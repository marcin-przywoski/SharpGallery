using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SharpGallery.Models;
using SharpGallery.Services;

namespace SharpGallery.ViewModels
{
    public partial class MainViewModel : ViewModelBase
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

        [ObservableProperty]
        private string _statusText = "Ready";

        [ObservableProperty]
        private string _searchText = "Enter search query";

        [ObservableProperty]
        private string _searchBarStatusText = "Ready";

        public UpdateViewModel UpdateViewModel { get; }

        /// <summary>
        /// Design-time constructor for XAML previewer.
        /// </summary>
        public MainViewModel() : this(new UpdateViewModel(), new ImageScanningService())
        {
        }

        public MainViewModel(UpdateViewModel updateViewModel, ImageScanningService scanningService)
        {
            _scanningService = scanningService;
            UpdateViewModel = updateViewModel;
        }

        [RelayCommand]
        public async Task LoadFolderAsync(string path)
        {
            if (string.IsNullOrEmpty(path))
                return;

            _statusText = "Scanning folder...";

            var items = await _scanningService.ScanDirectoryAsync(path);
            Images = new ObservableCollection<ImageItem>(items);
            FilteredImages = new ObservableCollection<ImageItem>(items);

            _statusText = $"Loaded {items.Count} images.";

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
                        // Assign the loaded thumbnail to the item; any UI updates depend on how ImageItem
                        // exposes property change notifications and is bound within the view.

                    }
                }
            }
        }

        partial void OnSearchTextChanged(string value)
        {
            PerformSearch(value);
        }

        private void PerformSearch(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                FilteredImages = new ObservableCollection<ImageItem>(Images);
                return;
            }

            FilteredImages = new ObservableCollection<ImageItem>(
                Images.Where(i => i.FileName.Contains(query, StringComparison.OrdinalIgnoreCase))
            );
        }
    }
}
