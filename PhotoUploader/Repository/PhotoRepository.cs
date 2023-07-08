using Microsoft.EntityFrameworkCore;
using PhotoUploader.Entities;

namespace PhotoUploader.Repository
{
    public class PhotoRepository : IPhotoRepository
    {
        private readonly PhotoDbContext _dbContext;

        public PhotoRepository(PhotoDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<Photo>> GetUserPhotosAsync(Guid userId)
        {
            return await _dbContext.Photos.Where(p => p.UserId == userId).OrderBy(p => p.DateCreated).ToListAsync();
        }
        
        public async Task<Photo> GetPhotoByIdAsync(Guid id)
        {
            return await _dbContext.Photos.FindAsync(id);
        }

        public async Task SavePhotoAsync(Photo photo)
        {
            var existingPhoto = await _dbContext.Photos.FindAsync(photo.Id);
            if (existingPhoto != null)
            {
                _dbContext.Photos.Update(photo);
            }
            else
            {
                await _dbContext.Photos.AddAsync(photo);
            }
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeletePhotoAsync(Guid id)
        {
            var photo = await _dbContext.Photos.FindAsync(id);
            if (photo != null)
            {
                _dbContext.Photos.Remove(photo);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task ClearMainPhotoAsync(Guid userId)
        {
            var mainPhoto = await _dbContext.Photos.Where(p => p.UserId == userId && p.IsMain == true).FirstOrDefaultAsync();
            if(mainPhoto != null)
            {
                mainPhoto.IsMain = false;
                _dbContext.Photos.Update(mainPhoto);
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}
