using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using SharpGallery.ViewModels;

namespace SharpGallery.Views
{
    public partial class MainView : Window
    {
        public MainView()
        {
            InitializeComponent();
        }

        private async void OpenFolder_Click(object? sender, RoutedEventArgs e)
        {
            var result = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Select Image Folder",
                AllowMultiple = false
            });

            if (result.Count > 0)
            {
                var path = result[0].Path.LocalPath;
                if (DataContext is MainViewModel vm)
                {
                    await vm.LoadFolderCommand.ExecuteAsync(path);
                }
            }
        }
    }
}