using PhotoUploader.Entities;

namespace PhotoUploader.Models
{
    public class GalleryViewModel
    {
        public List<PhotoViewModel> Photos { get; set; }
        public List<string> Tags { get; set; }
    }
}
