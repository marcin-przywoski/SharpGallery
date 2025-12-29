using System;
using System.Threading.Tasks;

namespace SharpGallery.Services
{
    public interface IUpdateService
    {
        Task<bool> CheckForUpdatesAsync();
        Task DownloadUpdateAsync(Action<int>? progressCallback = null);
        void ApplyUpdateAndRestart();
        void ApplyUpdateOnExit();
    }
}
