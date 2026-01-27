using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using SharpGallery.Models;

namespace SharpGallery.Services
{
    public class OcrService : IDisposable
    {
        private IOcrEngine? _currentEngine;
        private TesseractOcrEngine? _tesseractEngine;
        private PaddleOcrEngine? _paddleEngine;
        private OcrEngineType _currentEngineType = OcrEngineType.Tesseract;

        private readonly string TesseractDataFolder = Path.Combine(AppContext.BaseDirectory, "tessdata");

        public OcrEngineType CurrentEngineType
        {
            get => _currentEngineType;
            set
            {
                if (_currentEngineType != value)
                {
                    _currentEngineType = value;
                    _currentEngine = null; // Force re-initialization
                }
            }
        }

        public async Task InitializeAsync(OcrEngineType engineType)
        {
            CurrentEngineType = engineType;
            
            switch (engineType)
            {
                case OcrEngineType.Tesseract:
                    if (_tesseractEngine == null)
                    {
                        _tesseractEngine = new TesseractOcrEngine();
                    }
                    if (!_tesseractEngine.IsLoaded)
                    {
                        await _tesseractEngine.InitializeAsync(TesseractDataFolder);
                    }
                    _currentEngine = _tesseractEngine;
                    break;

                case OcrEngineType.PaddleOCR:
                    if (_paddleEngine == null)
                    {
                        _paddleEngine = new PaddleOcrEngine();
                    }
                    if (!_paddleEngine.IsLoaded)
                    {
                        // PaddleOCR doesn't use the data path, passing empty string
                        await _paddleEngine.InitializeAsync(string.Empty);
                    }
                    _currentEngine = _paddleEngine;
                    break;
            }
        }

        public async Task ProcessImagesAsync(List<ImageItem> images)
        {
            if (_currentEngine == null || !_currentEngine.IsLoaded)
            {
                await InitializeAsync(_currentEngineType);
                if (_currentEngine == null || !_currentEngine.IsLoaded)
                    return;
            }

            await _currentEngine.ProcessImagesAsync(images);
        }

        public void Dispose()
        {
            _tesseractEngine?.Dispose();
            _paddleEngine?.Dispose();
        }
    }
}
