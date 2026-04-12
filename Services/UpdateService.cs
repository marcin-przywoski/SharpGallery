using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using SharpGallery.Models;
using Velopack;
using Velopack.Sources;

namespace SharpGallery.Services
{
    public class UpdateService : IUpdateService
    {
        private readonly UpdateManager _updateManager;
        private readonly HttpClient _httpClient;
        private UpdateInfo? _updateInfo;

        private const string GitHubRepoUrl = "https://github.com/marcin-przywoski/SharpGallery";
        private const string GitHubApiReleasesUrl = "https://api.github.com/repos/marcin-przywoski/SharpGallery/releases";

        public string? LatestVersion { get; private set; }
        public string? ReleaseNotes { get; private set; }
        public string? ReleaseName { get; private set; }
        public DateTime? ReleaseDate { get; private set; }

        public UpdateService()
        {
            var source = new GithubSource(GitHubRepoUrl, null, false);
            _updateManager = new UpdateManager(source);
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "SharpGallery-UpdateChecker");
        }

        public async Task<bool> CheckForUpdatesAsync()
        {
            try
            {
                if (!_updateManager.IsInstalled)
                {
                    System.Diagnostics.Debug.WriteLine("[UpdateService] App is not installed via Velopack (dev mode). Skipping Velopack check, falling back to GitHub API.");
                    return await CheckForUpdatesViaGitHubAsync();
                }

                _updateInfo = await _updateManager.CheckForUpdatesAsync();

                if (_updateInfo != null)
                {
                    LatestVersion = _updateInfo.TargetFullRelease?.Version?.ToString();
                    await FetchReleaseDetailsAsync(LatestVersion);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[UpdateService] Velopack check failed: {ex.Message}. Falling back to GitHub API.");
                return await CheckForUpdatesViaGitHubAsync();
            }
        }

        private async Task<bool> CheckForUpdatesViaGitHubAsync()
        {
            try
            {
                var response = await _httpClient.GetStringAsync(GitHubApiReleasesUrl);
                var releases = JsonSerializer.Deserialize<GitHubRelease[]>(response);

                if (releases == null || releases.Length == 0)
                    return false;

                var latest = releases[0];
                var latestTag = latest.TagName?.TrimStart('v', 'V');

                if (string.IsNullOrEmpty(latestTag))
                    return false;

                // Compare with current version
                if (Version.TryParse(latestTag, out var remoteVersion) &&
                    Version.TryParse(BuildInfo.Version, out var localVersion) &&
                    remoteVersion > localVersion)
                {
                    LatestVersion = latestTag;
                    ReleaseName = latest.Name;
                    ReleaseDate = latest.PublishedAt;
                    ReleaseNotes = AggregateReleaseNotes(releases, localVersion);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[UpdateService] GitHub API check also failed: {ex.Message}");
                return false;
            }
        }

        private async Task FetchReleaseDetailsAsync(string? version)
        {
            if (string.IsNullOrEmpty(version))
                return;

            try
            {
                var response = await _httpClient.GetStringAsync(GitHubApiReleasesUrl);
                var releases = JsonSerializer.Deserialize<GitHubRelease[]>(response);

                if (releases == null || releases.Length == 0)
                    return;

                // Use the latest release for name/date
                var latest = releases[0];
                ReleaseName = latest.Name;
                ReleaseDate = latest.PublishedAt;

                // Aggregate notes from all releases newer than current version
                if (Version.TryParse(BuildInfo.Version, out var localVersion))
                {
                    ReleaseNotes = AggregateReleaseNotes(releases, localVersion);
                }
                else
                {
                    // Fallback: just show the matching release notes
                    var matchingRelease = releases.FirstOrDefault(r =>
                        r.TagName != null && r.TagName.TrimStart('v', 'V') == version);
                    ReleaseNotes = matchingRelease?.Body ?? latest.Body;
                }
            }
            catch (Exception)
            {
                // Silently fail - release notes are optional
                ReleaseNotes = null;
                ReleaseName = null;
                ReleaseDate = null;
            }
        }

        private static string AggregateReleaseNotes(GitHubRelease[] releases, Version localVersion)
        {
            var newerReleases = releases
                .Where(r => r.TagName != null &&
                            Version.TryParse(r.TagName.TrimStart('v', 'V'), out var v) &&
                            v > localVersion)
                .OrderByDescending(r =>
                {
                    Version.TryParse(r.TagName!.TrimStart('v', 'V'), out var v);
                    return v;
                })
                .ToList();

            if (newerReleases.Count == 0)
                return releases[0].Body ?? string.Empty;

            if (newerReleases.Count == 1)
                return newerReleases[0].Body ?? string.Empty;

            var sb = new System.Text.StringBuilder();
            foreach (var release in newerReleases)
            {
                var tag = release.TagName?.TrimStart('v', 'V');
                var header = !string.IsNullOrEmpty(release.Name)
                    ? $"── {release.Name} (v{tag})"
                    : $"── v{tag}";

                sb.AppendLine(header);
                if (!string.IsNullOrWhiteSpace(release.Body))
                {
                    sb.AppendLine(release.Body.Trim());
                }
                sb.AppendLine();
            }

            return sb.ToString().TrimEnd();
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
