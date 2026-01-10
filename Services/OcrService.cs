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
    }
    
}
