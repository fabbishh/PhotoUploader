using PhotoUploader.Entities;
using PhotoUploader.Models;

namespace PhotoUploader.Services
{
    public interface IPhotoService
    {
        Task<List<Photo>> GetPhotosAsync();
        Task<FilesResponseModel> UploadPhotoAsync(UploadPhotosModel model, int maxFiles);
        Task SetMainPhotoAsync(Guid id);
        Task DeletePhotoAsync(Guid id);
    }
}
