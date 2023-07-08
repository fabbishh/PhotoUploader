using PhotoUploader.Entities;

namespace PhotoUploader.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly PhotoDbContext _dbContext;

        public UserRepository(PhotoDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IQueryable<User> GetAll()
        {
            return _dbContext.Users;
        }

        public async Task Delete(User entity)
        {
            _dbContext.Users.Remove(entity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task Create(User entity)
        {
            await _dbContext.Users.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<User> Update(User entity)
        {
            _dbContext.Users.Update(entity);
            await _dbContext.SaveChangesAsync();

            return entity;
        }
    }
}
