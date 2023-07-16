using PhotoUploader.Entities;

namespace PhotoUploader.Repository
{
    public interface IPhotoRepository
    {
        Task<List<Photo>> GetUserPhotosAsync(Guid userId);
        Task<Photo> GetPhotoByIdAsync(Guid id);
        Task SavePhotoAsync(Photo photo);
        Task DeletePhotoAsync(Guid id);
        Task ClearMainPhotoAsync(Guid userId);
        Task<bool> CheckPhotoExistsAsync(string hash, Guid userId);
    }
}
