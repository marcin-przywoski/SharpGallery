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
        public MainWindowViewModel() : this(new UpdateViewModel())
        {
        }

        public MainWindowViewModel(UpdateViewModel updateViewModel)
        {
            UpdateViewModel = updateViewModel;
        }
    }
}