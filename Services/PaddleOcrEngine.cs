using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using SharpGallery.Models;
using PaddleOCRSharp;

namespace SharpGallery.Services
{
    public class PaddleOcrEngine : IOcrEngine, IDisposable
    {
        private PaddleOCREngine? _engine;
        private bool _isLoaded;

        public bool IsLoaded => _isLoaded;

        public async Task InitializeAsync(string dataPath)
        {
            if (_isLoaded)
                return;

            await Task.Run(() =>
            {
                try
                {
                    // Initialize PaddleOCR with default models
                    // PaddleOCRSharp will handle model downloading automatically
                    OCRModelConfig config = null;
                    
                    // Try to use local models if they exist, otherwise use default
                    string modelPath = Path.Combine(dataPath, "paddle_models");
                    if (Directory.Exists(modelPath))
                    {
                        // Use local models if available
                        OCRParameter oCRParameter = new OCRParameter();
                        oCRParameter.enable_mkldnn = 1; // Enable MKLDNN for better performance
                        oCRParameter.cpu_math_library_num_threads = 6;
                        config = new OCRModelConfig();
                        // Set custom model paths if needed
                    }
                    else
                    {
                        // Use default built-in models
                        OCRParameter oCRParameter = new OCRParameter();
                        oCRParameter.enable_mkldnn = 1;
                        oCRParameter.cpu_math_library_num_threads = 6;
                        config = new OCRModelConfig();
                    }

                    _engine = new PaddleOCREngine(config, new OCRParameter());
                    _isLoaded = true;
                    Console.WriteLine("PaddleOCR initialized successfully.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to init PaddleOCR: {ex.Message}");
                }
            });
        }

        public async Task ProcessImagesAsync(List<ImageItem> images)
        {
            if (!_isLoaded || _engine == null)
                return;

            await Task.Run(() =>
            {
                foreach (var img in images)
                {
                    // Skip if already has text
                    if (!string.IsNullOrEmpty(img.OcrText))
                        continue;

                    try
                    {
                        OCRResult result = _engine.DetectText(img.Path);
                        
                        if (result != null && result.TextBlocks != null && result.TextBlocks.Count > 0)
                        {
                            // Combine all text blocks into a single string
                            var textParts = new List<string>();
                            foreach (var block in result.TextBlocks)
                            {
                                if (!string.IsNullOrWhiteSpace(block.Text))
                                {
                                    textParts.Add(block.Text.Trim());
                                }
                            }
                            
                            if (textParts.Count > 0)
                            {
                                img.OcrText = string.Join(" ", textParts);
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
            _engine?.Dispose();
        }
    }
}
