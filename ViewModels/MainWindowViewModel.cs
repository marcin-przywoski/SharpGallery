using SharpGallery.Services;

namespace SharpGallery.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        public string Greeting { get; } = "Welcome to Avalonia!";

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