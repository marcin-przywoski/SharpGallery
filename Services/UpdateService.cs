using System;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Velopack;
using Velopack.Sources;

namespace SharpGallery.Services
{
    public class UpdateService : IUpdateService
    {
        private readonly UpdateManager _updateManager;
        private UpdateInfo? _updateInfo;

        private const string GitHubRepoUrl = "https://github.com/marcin-przywoski/SharpGallery";

        public UpdateService()
        {
            var source = new GithubSource(GitHubRepoUrl, null, false);
            _updateManager = new UpdateManager(source);
        }

        public async Task<bool> CheckForUpdatesAsync()
        {
            try
            {
                _updateInfo = await _updateManager.CheckForUpdatesAsync();

                return _updateInfo != null;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task DownloadUpdateAsync(Action<int>? progressCallback = null)
        {
            if (_updateInfo == null)
                return;

            await _updateManager.DownloadUpdatesAsync(_updateInfo, progress =>
            {
                progressCallback?.Invoke(progress);
            });
        }

        public void ApplyUpdateAndRestart()
        {
            if (_updateInfo == null)
                return;

            _updateManager.ApplyUpdatesAndRestart();
        }

        public void ApplyUpdateOnExit()
        {
            if (_updateInfo == null)
                return;

            _updateManager.ApplyUpdatesAndExit();
        }
    }
}
