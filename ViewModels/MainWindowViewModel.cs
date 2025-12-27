namespace SharpGallery.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        public string Greeting { get; } = "Welcome to Avalonia!";

        public UpdateViewModel UpdateViewModel { get; } = new UpdateViewModel();

        public async void InitializeAsync()
        {
            await UpdateViewModel.CheckForUpdatesOnStartupAsync();
        }
    }
}