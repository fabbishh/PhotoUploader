using PhotoUploader.Entities;

namespace PhotoUploader.Repository
{
    public class TagRepository : ITagRepository
    {
        private readonly PhotoDbContext _dbContext;
        public TagRepository(PhotoDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IQueryable<Tag> GetAll()
        {
            return _dbContext.Tags;
        }
    }
}
