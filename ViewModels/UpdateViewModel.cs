using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SharpGallery.Services;

namespace SharpGallery.ViewModels
{
    public partial class UpdateViewModel : ViewModelBase
    {
        private readonly UpdateService _updateService;

        [ObservableProperty]
        private bool _isCheckingForUpdates;

        [ObservableProperty]
        private bool _isUpdateAvailable;

        [ObservableProperty]
        private bool _isDownloading;

        [ObservableProperty]
        private int _downloadProgress;

        [ObservableProperty]
        private bool _isReadyToInstall;

        [ObservableProperty]
        private string _statusMessage = string.Empty;

        [ObservableProperty]
        private string? _currentVersion;

        [ObservableProperty]
        private string? _newVersion;

        [ObservableProperty]
        private string? _releaseNotes;

        [ObservableProperty]
        private string? _releaseName;

        [ObservableProperty]
        private DateTime? _releaseDate;

        public UpdateViewModel()
        {
            _updateService = new UpdateService();
        }

        [RelayCommand]
        private async Task CheckForUpdatesAsync()
        {
            if (IsCheckingForUpdates || IsDownloading)
                return;

            IsCheckingForUpdates = true;
            StatusMessage = "Checking for updates...";
            IsUpdateAvailable = false;
            IsReadyToInstall = false;

            try
            {
                var hasUpdate = await _updateService.CheckForUpdatesAsync();

                if (hasUpdate)
                {
                    IsUpdateAvailable = true;
                    StatusMessage = "Update available!";
                }
                else
                {
                    StatusMessage = "You're up to date!";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error checking for updates: {ex.Message}";
            }
            finally
            {
                IsCheckingForUpdates = false;
            }
        }

        [RelayCommand]
        private async Task DownloadUpdateAsync()
        {
            if (IsDownloading || !IsUpdateAvailable)
                return;

            IsDownloading = true;
            DownloadProgress = 0;
            StatusMessage = "Downloading update...";

            try
            {
                await _updateService.DownloadUpdateAsync(progress =>
                {
                    DownloadProgress = progress;
                    StatusMessage = $"Downloading update... {progress}%";
                });

                IsReadyToInstall = true;
                IsUpdateAvailable = false;
                StatusMessage = "Update downloaded. Ready to install!";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error downloading update: {ex.Message}";
            }
            finally
            {
                IsDownloading = false;
            }
        }

        [RelayCommand]
        private void InstallAndRestart()
        {
            if (!IsReadyToInstall)
                return;

            StatusMessage = "Installing update and restarting...";
            _updateService.ApplyUpdateAndRestart();
        }

        [RelayCommand]
        private void InstallOnExit()
        {
            if (!IsReadyToInstall)
                return;

            StatusMessage = "Update will be installed when you close the app.";
            _updateService.ApplyUpdateOnExit();
        }

        public async Task CheckForUpdatesOnStartupAsync()
        {
            await CheckForUpdatesAsync();
        }
    }
}
