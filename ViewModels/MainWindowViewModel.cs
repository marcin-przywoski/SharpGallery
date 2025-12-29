using SharpGallery.Services;

namespace SharpGallery.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        public string Greeting { get; } = "Welcome to Avalonia!";

        public UpdateViewModel UpdateViewModel { get; }

        public MainWindowViewModel(UpdateViewModel updateViewModel)
        {
            UpdateViewModel = updateViewModel;
        }
    }
}