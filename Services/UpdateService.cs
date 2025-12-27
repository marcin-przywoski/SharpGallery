using System;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Velopack;
using Velopack.Sources;

namespace SharpGallery.Services
{
    public class GitHubReleaseInfo
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("tag_name")]
        public string? TagName { get; set; }

        [JsonPropertyName("body")]
        public string? Body { get; set; }

        [JsonPropertyName("published_at")]
        public DateTime? PublishedAt { get; set; }

        [JsonPropertyName("html_url")]
        public string? HtmlUrl { get; set; }
    }

    public class UpdateService
    {
        private readonly UpdateManager _updateManager;
        private readonly HttpClient _httpClient;
        private UpdateInfo? _updateInfo;
        private GitHubReleaseInfo? _latestReleaseInfo;

        private const string GitHubRepoUrl = "https://github.com/marcin-przywoski/SharpGallery";
        private const string GitHubApiUrl = "https://api.github.com/repos/marcin-przywoski/SharpGallery/releases/latest";

        public UpdateService()
        {
            var source = new GithubSource(GitHubRepoUrl, null, false);
            _updateManager = new UpdateManager(source);
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "SharpGallery");
        }

        public bool IsInstalled => _updateManager.IsInstalled;

        public string? CurrentVersion => _updateManager.CurrentVersion?.ToString();

        public string? NewVersion => _updateInfo?.TargetFullRelease?.Version?.ToString();

        public string? ReleaseNotes => _latestReleaseInfo?.Body;

        public string? ReleaseName => _latestReleaseInfo?.Name;

        public DateTime? ReleaseDate => _latestReleaseInfo?.PublishedAt;

        public string? ReleaseUrl => _latestReleaseInfo?.HtmlUrl;

        public async Task<bool> CheckForUpdatesAsync()
        {
            if (!IsInstalled)
                return false;

            try
            {
                _updateInfo = await _updateManager.CheckForUpdatesAsync();

                if (_updateInfo != null)
                {
                    await FetchReleaseInfoAsync();
                }

                return _updateInfo != null;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private async Task FetchReleaseInfoAsync()
        {
            try
            {
                var response = await _httpClient.GetStringAsync(GitHubApiUrl);
                _latestReleaseInfo = JsonSerializer.Deserialize<GitHubReleaseInfo>(response);
            }
            catch
            {
                _latestReleaseInfo = null;
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
