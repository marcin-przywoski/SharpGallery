using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using SharpGallery.Models;
using Tesseract;

namespace SharpGallery.Services
{
    public class OcrService
    {
        private TesseractEngine? _engine;
        private bool _isLoaded;
        private const string TesseractDataUrl = "https://github.com/tesseract-ocr/tessdata_best/raw/main/eng.traineddata";

        private readonly string ApplicationFolder = AppContext.BaseDirectory + @"\tessdata";

        public async Task InitializeAsync(string dataPath)
        {
            if (_isLoaded)
                return;

            await Task.Run(async () =>
            {
                try
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
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to init OCR: {ex.Message}");
                }
            });
        }

    }
    
}
