using PhotoUploader.Entities;

namespace PhotoUploader.Repository
{
    public interface ITagRepository
    {
        IQueryable<Tag> GetAll();
    }
}
