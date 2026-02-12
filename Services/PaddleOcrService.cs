using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using OpenCvSharp;
using SharpGallery.Models;
using Sdcb.PaddleInference;
using Sdcb.PaddleOCR;
using Sdcb.PaddleOCR.Models;
using Sdcb.PaddleOCR.Models.Local;

namespace SharpGallery.Services
{
    public class PaddleOcrService : IDisposable
    {
        private PaddleOcrAll? _ocrEngine;
        private bool _isLoaded;

        public async Task InitializeAsync()
        {
            if (_isLoaded)
                return;

            await Task.Run(() =>
            {
                try
                {
                    Console.WriteLine("Initializing PaddleOCR...");
                    
                    // Use the LocalV3 English model
                    FullOcrModel model = LocalFullModels.EnglishV3;
                    
                    // Initialize with CPU mode (Mkldnn for better performance)
                    _ocrEngine = new PaddleOcrAll(model, PaddleDevice.Mkldnn())
                    {
                        AllowRotateDetection = true, // Enable text rotation detection
                        Enable180Classification = false // Disable 180-degree classification for better performance
                    };
                    
                    _isLoaded = true;
                    Console.WriteLine("PaddleOCR initialized successfully.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to initialize PaddleOCR: {ex.Message}");
                }
            });
        }

        public async Task ProcessImagesAsync(List<ImageItem> images)
        {
            if (!_isLoaded)
            {
                await InitializeAsync();
                if (!_isLoaded)
                    return;
            }

            await Task.Run(() =>
            {
                foreach (var img in images)
                {
                    // Skip if already has text
                    if (!string.IsNullOrEmpty(img.OcrText))
                        continue;

                    try
                    {
                        // Load image using OpenCV
                        byte[] imageData = File.ReadAllBytes(img.Path);
                        using (Mat mat = Cv2.ImDecode(imageData, ImreadModes.Color))
                        {
                            if (mat.Empty())
                            {
                                Console.WriteLine($"Failed to load image: {img.FileName}");
                                continue;
                            }

                            var result = _ocrEngine!.Run(mat);
                            
                            if (result != null && result.Regions.Any())
                            {
                                // Concatenate all detected text with newlines
                                string text = string.Join("\n", result.Regions.Select(r => r.Text));
                                
                                if (!string.IsNullOrWhiteSpace(text))
                                {
                                    img.OcrText = text.Trim();
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"PaddleOCR failed for {img.FileName}: {ex.Message}");
                    }
                }
            });
        }

        public void Dispose()
        {
            _ocrEngine?.Dispose();
        }
    }
}
