# Sharp Gallery

[![CI](https://github.com/marcin-przywoski/SharpGallery/actions/workflows/CI.yml/badge.svg)](https://github.com/marcin-przywoski/SharpGallery/actions/workflows/CI.yml)
[![Release](https://img.shields.io/github/release/marcin-przywoski/SharpGallery.svg)](https://github.com/marcin-przywoski/SharpGallery/releases)
![GitHub code size in bytes](https://img.shields.io/github/languages/code-size/marcin-przywoski/SharpGallery)
[![Downloads](https://img.shields.io/github/downloads/marcin-przywoski/SharpGallery/total)](https://github.com/marcin-przywoski/SharpGallery/releases)

This application serves as a gallery for photos meant down the line to be a replacement for Windows Photos while providing better performance and features.

## Features

- Image gallery with thumbnail support
- OCR text recognition with two engine options:
  - **PaddleOCR** (default) - Fast and accurate OCR using PaddleOCR V3 models
  - **Tesseract** - Traditional OCR engine
- Text search across image filenames and OCR content
- Auto-update functionality

## Installation

Download the repository and compile it by yourself in VS Code or Visual Studio or download the compiled package from the Packages section

## Requirements

- .NET 3.1 SDK if you want to compile it for yourself

## OCR Engines

SharpGallery now supports two OCR engines:

### PaddleOCR (Default)
- Uses the EnglishV3 model from PaddleOCR
- Provides better accuracy for text detection
- Supports rotated text detection
- CPU-optimized with MKL runtime

### Tesseract
- Legacy OCR engine
- Downloads language data on first use
- Can be selected by changing `SelectedEngine` in OcrService

## Acknowledgments

- [Avalonia UI](https://github.com/AvaloniaUI/Avalonia)
- [Velopack](https://github.com/velopack/velopack)
- [PaddleOCR](https://github.com/PaddlePaddle/PaddleOCR)
- [Sdcb.PaddleSharp](https://github.com/sdcb/PaddleSharp)
- [Tesseract OCR](https://github.com/tesseract-ocr/tesseract)
