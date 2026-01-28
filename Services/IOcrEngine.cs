using System.Collections.Generic;
using System.Threading.Tasks;
using SharpGallery.Models;

namespace SharpGallery.Services
{
    public interface IOcrEngine
    {
        Task InitializeAsync(string dataPath);
        Task ProcessImagesAsync(List<ImageItem> images);
        bool IsLoaded { get; }
    }
}
