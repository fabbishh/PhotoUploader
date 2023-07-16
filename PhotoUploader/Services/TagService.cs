using Microsoft.EntityFrameworkCore;
using PhotoUploader.Entities;
using PhotoUploader.Repository;

namespace PhotoUploader.Services
{
    public class TagService : ITagService
    {
        private readonly ITagRepository _tagRepository;
        public TagService(ITagRepository tagRepository)
        {
            _tagRepository = tagRepository;
        }

        public async Task<List<Tag>> GetTagsAsync(string searchTag)
        {
            return await _tagRepository
                .GetAll()
                .Where(t => t.Name.Contains(searchTag))
                .ToListAsync();
        }
    }
}
