using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
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
        private readonly SemaphoreSlim _initLock = new SemaphoreSlim(1, 1);
        private readonly object _lock = new object();
        private const string TesseractDataUrl = "https://github.com/tesseract-ocr/tessdata_best/raw/main/eng.traineddata";

        private readonly string ApplicationFolder = Path.Combine(AppContext.BaseDirectory, "tessdata");
        
        public OcrEngine SelectedEngine { get; set; } = OcrEngine.PaddleOCR;

        public async Task InitializeAsync(string dataPath)
        {
            await _initLock.WaitAsync();
            try
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

                            var engine = new TesseractEngine(dataPath, "eng", EngineMode.LstmOnly);
                            
                            lock (_lock)
                            {
                                _engine = engine;
                                _isLoaded = true;
                            }
                        }
                        else if (SelectedEngine == OcrEngine.PaddleOCR)
                        {
                            var paddleService = new PaddleOcrService();
                            await paddleService.InitializeAsync();
                            
                            lock (_lock)
                            {
                                _paddleOcrService = paddleService;
                                _isLoaded = true;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to init OCR: {ex.Message}");
                    }
                });
            }
            finally
            {
                _initLock.Release();
            }
        }

        public async Task ProcessImagesAsync(List<ImageItem> images)
        {
            bool isLoaded;
            lock (_lock)
            {
                isLoaded = _isLoaded;
            }

            if (!isLoaded)
            {
                await InitializeAsync(ApplicationFolder);
                lock (_lock)
                {
                    isLoaded = _isLoaded;
                }
                if (!isLoaded)
                    return;
            }

            PaddleOcrService? paddleService = null;
            TesseractEngine? tesseractEngine = null;
            
            lock (_lock)
            {
                paddleService = _paddleOcrService;
                tesseractEngine = _engine;
            }

            if (SelectedEngine == OcrEngine.PaddleOCR && paddleService != null)
            {
                await paddleService.ProcessImagesAsync(images);
            }
            else if (SelectedEngine == OcrEngine.Tesseract && tesseractEngine != null)
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
                            using var page = tesseractEngine.Process(pix);
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
            lock (_lock)
            {
                _engine?.Dispose();
                _engine = null;
                _paddleOcrService?.Dispose();
                _paddleOcrService = null;
                _isLoaded = false;
            }
            _initLock?.Dispose();
        }
    }

}
