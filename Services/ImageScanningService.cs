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

        public async Task<List<ImageItem>> ScanDirectoryAsync(string path)
        {
            return await Task.Run(() =>
            {
                var images = new List<ImageItem>();
                if (!Directory.Exists(path))
                    return images;

                var files = Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories)
                    .Where(s => _supportedExtensions.Contains(Path.GetExtension(s).ToLowerInvariant()));

                foreach (var file in files)
                {
                    images.Add(new ImageItem
                    {
                        Path = file,
                        FileName = Path.GetFileName(file)
                    });
                }
                return images;
            });
        }

    }
}
