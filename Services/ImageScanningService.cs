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

        public async Task<Bitmap?> LoadThumbnailAsync(string path, int width = 200)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using var stream = File.OpenRead(path);
                    // Use SkiaSharp for efficient resizing
                    using var original = SKBitmap.Decode(stream);
                    if (original == null)
                        return null;

                    int height = (int)((float)original.Height / original.Width * width);
                    using var resized = original.Resize(new SKImageInfo(width, height), SKFilterQuality.High);
                    using var image = SKImage.FromBitmap(resized);
                    using var data = image.Encode(SKEncodedImageFormat.Jpeg, 90);

                    return new Bitmap(data.AsStream());
                }
                catch (IOException ex)
                {
                    Console.Error.WriteLine($"Failed to load thumbnail for '{path}': {ex}");
                    return null;
                }
                catch (UnauthorizedAccessException ex)
                {
                    Console.Error.WriteLine($"Access denied while loading thumbnail for '{path}': {ex}");
                    return null;
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Unexpected error while loading thumbnail for '{path}': {ex}");
                    return null;
                }
            });
        }
    }
}
