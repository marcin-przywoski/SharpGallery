using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using SharpGallery.Models;
using Tesseract;

namespace SharpGallery.Services
{
    public enum OcrEngine
    {
        Tesseract,
        PaddleOCR
    }

    public class OcrService : IDisposable
    {
        private TesseractEngine? _engine;
        private PaddleOcrService? _paddleOcrService;
        private bool _isLoaded;
        private const string TesseractDataUrl = "https://github.com/tesseract-ocr/tessdata_best/raw/main/eng.traineddata";

        private readonly string ApplicationFolder = Path.Combine(AppContext.BaseDirectory, "tessdata");
        
        public OcrEngine SelectedEngine { get; set; } = OcrEngine.PaddleOCR;

        public async Task InitializeAsync(string dataPath)
        {
            if (_isLoaded)
                return;

            await Task.Run(async () =>
            {
                try
                {
                    if (SelectedEngine == OcrEngine.Tesseract)
                    {
                        if (!Directory.Exists(dataPath))
                        {
                            Directory.CreateDirectory(dataPath);
                        }

                        string tesseractFile = Path.Combine(dataPath, "eng.traineddata");

                        if (!File.Exists(tesseractFile))
                        {
                            using var client = new HttpClient();
                            Console.WriteLine("Downloading Tesseract data...");
                            using var response = await client.GetAsync(TesseractDataUrl);
                            response.EnsureSuccessStatusCode();
                            using var stream = await response.Content.ReadAsStreamAsync();
                            using var fileStream = File.Create(tesseractFile);
                            await stream.CopyToAsync(fileStream);
                            Console.WriteLine("Downloaded Tesseract data.");
                        }

                        _engine = new TesseractEngine(dataPath, "eng", EngineMode.LstmOnly);
                        _isLoaded = true;
                    }
                    else if (SelectedEngine == OcrEngine.PaddleOCR)
                    {
                        _paddleOcrService = new PaddleOcrService();
                        await _paddleOcrService.InitializeAsync();
                        _isLoaded = true;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to init OCR: {ex.Message}");
                }
            });
        }

        public async Task ProcessImagesAsync(List<ImageItem> images)
        {
            if (!_isLoaded)
            {
                await InitializeAsync(ApplicationFolder);
                if (!_isLoaded)
                    return;
            }

            if (SelectedEngine == OcrEngine.PaddleOCR && _paddleOcrService != null)
            {
                await _paddleOcrService.ProcessImagesAsync(images);
            }
            else if (SelectedEngine == OcrEngine.Tesseract && _engine != null)
            {
                await Task.Run(() =>
                {
                    foreach (var img in images)
                    {
                        // Skip if already has text
                        if (!string.IsNullOrEmpty(img.OcrText))
                            continue;

                        try
                        {
                            using var pix = Pix.LoadFromFile(img.Path);
                            using var page = _engine!.Process(pix);
                            string text = page.GetText();

                            if (!string.IsNullOrWhiteSpace(text))
                            {
                                img.OcrText = text.Trim();
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"OCR failed for {img.FileName}: {ex.Message}");
                        }
                    }
                });
            }
        }

        public void Dispose()
        {
            _engine?.Dispose();
            _paddleOcrService?.Dispose();
        }
    }

}
