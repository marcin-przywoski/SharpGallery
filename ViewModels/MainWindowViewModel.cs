using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SharpGallery.Models;
using SharpGallery.Services;

namespace SharpGallery.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private readonly ImageScanningService _scanningService;

        [ObservableProperty]
        private ObservableCollection<ImageItem> _images = new();

        [ObservableProperty]
        private ObservableCollection<ImageItem> _filteredImages = new();

        [ObservableProperty]
        private ImageItem? _selectedImage;

        [ObservableProperty]
        private bool _isGalleryView = true;

        public UpdateViewModel UpdateViewModel { get; }

        /// <summary>
        /// Design-time constructor for XAML previewer.
        /// </summary>
        public MainWindowViewModel() : this(new UpdateViewModel(), new ImageScanningService())
        {
        }

        public MainWindowViewModel(UpdateViewModel updateViewModel, ImageScanningService scanningService)
        {
            _scanningService = scanningService;
            UpdateViewModel = updateViewModel;
        }

        [RelayCommand]
        public async Task LoadFolderAsync(string path)
        {
            if (string.IsNullOrEmpty(path))
                return;

            var items = await _scanningService.ScanDirectoryAsync(path);
            Images = new ObservableCollection<ImageItem>(items);
            FilteredImages = new ObservableCollection<ImageItem>(items);

            // Trigger background thumbnail loading
            _ = LoadThumbnailsAsync(items);
        }

        private async Task LoadThumbnailsAsync(List<ImageItem> items)
        {
            foreach (var item in items)
            {
                if (item.Thumbnail == null)
                {
                    var thumb = await _scanningService.LoadThumbnailAsync(item.Path);
                    if (thumb != null)
                    {
                        item.Thumbnail = thumb;
                        // Force update if necessary, but replacing the item in ObservableCollection is cleaner
                        // For now, we assume View binds to Property and Property Changed event needed on ImageItem
                        // To make it simple, we will assume ImageItem needs to be observable.
                    }
                }
            }
        }
    }
}
