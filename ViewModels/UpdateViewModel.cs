using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SharpGallery.Services;

namespace SharpGallery.ViewModels
{
    public partial class UpdateViewModel : ViewModelBase
    {
        private readonly IUpdateService _updateService;

        [ObservableProperty]
        private bool _isCheckingForUpdates = false;

        [ObservableProperty]
        private bool _isUpdateAvailable = false;

        [ObservableProperty]
        private bool _isDownloading = false;

        [ObservableProperty]
        private int _downloadProgress = 0;

        [ObservableProperty]
        private bool _isReadyToInstall = false;

        [ObservableProperty]
        private string _statusMessage = string.Empty;

        [ObservableProperty]
        private string? _currentVersion = string.Empty;

        [ObservableProperty]
        private string? _newVersion = string.Empty;

        [ObservableProperty]
        private string? _releaseNotes = string.Empty;

        [ObservableProperty]
        private string? _releaseName = string.Empty;

        [ObservableProperty]
        private DateTime? _releaseDate = DateTime.Now;

        /// <summary>
        /// Whether the update panel should be visible.
        /// True when an update is available (or downloading/ready to install) and user hasn't dismissed it.
        /// </summary>
        [ObservableProperty]
        private bool _isUpdatePanelVisible = false;

        /// <summary>
        /// Design-time constructor for XAML previewer.
        /// </summary>
        public UpdateViewModel()
        {
            _updateService = null!;
        }

        public UpdateViewModel(IUpdateService updateService)
        {
            _updateService = updateService;
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
                    NewVersion = _updateService.LatestVersion;
                    ReleaseNotes = _updateService.ReleaseNotes;
                    ReleaseName = _updateService.ReleaseName;
                    ReleaseDate = _updateService.ReleaseDate;
                    CurrentVersion = BuildInfo.Version;
                    StatusMessage = $"Version {NewVersion} is available!";
                    IsUpdatePanelVisible = true;
                }
                else
                {
                    StatusMessage = "You're up to date!";
                    IsUpdatePanelVisible = false;
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error checking for updates: {ex.Message}";
                IsUpdatePanelVisible = false;
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
            IsUpdatePanelVisible = false;
            _updateService.ApplyUpdateOnExit();
        }

        [RelayCommand]
        private void DismissUpdate()
        {
            IsUpdatePanelVisible = false;
        }

        public async Task CheckForUpdatesOnStartupAsync()
        {
            await CheckForUpdatesAsync();
        }
    }
}
