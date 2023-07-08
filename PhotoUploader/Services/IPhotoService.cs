using PhotoUploader.Entities;

namespace PhotoUploader.Services
{
    public interface IPhotoService
    {
        Task<List<Photo>> GetPhotosAsync();
        Task UploadPhotoAsync(List<IFormFile> files);
        Task SetMainPhotoAsync(Guid id);
        Task DeletePhotoAsync(Guid id);
        bool IsSupportedFileType(string fileName);
    }
}
