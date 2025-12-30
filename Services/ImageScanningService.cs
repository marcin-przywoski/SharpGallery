using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using SharpGallery.Models;
using SkiaSharp;

namespace SharpGallery.Services
{
    public class ImageScanningService
    {
        private readonly string[] _supportedExtensions = { ".jpg", ".jpeg", ".png", ".bmp", ".webp" };

    }
}
