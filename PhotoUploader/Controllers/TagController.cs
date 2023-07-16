using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhotoUploader.Entities;
using PhotoUploader.Services;

namespace PhotoUploader.Controllers
{
    [Authorize]
    public class TagController
    {
        private readonly ITagService _tagService;
        public TagController(ITagService tagService)
        {
            _tagService = tagService;
        }

        [HttpGet]
        public async Task<IEnumerable<Tag>> GetAllTags(string searchTerm)
        {
            searchTerm = searchTerm?.Trim().ToLowerInvariant();
            var tags = await _tagService.GetTagsAsync(searchTerm);
            return tags;
        }
    }
}
