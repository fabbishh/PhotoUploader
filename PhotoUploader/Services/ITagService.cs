using PhotoUploader.Entities;

namespace PhotoUploader.Services
{
    public interface ITagService
    {
        Task<List<Tag>> GetTagsAsync(string searchTag);
    }
}
