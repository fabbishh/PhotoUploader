using PhotoUploader.Entities;

namespace PhotoUploader.Models
{
    public class FilesResponseModel
    {
        public List<string> ValidFiles = new List<string>();
        public List<string> DuplicateFiles = new List<string>();
        public List<string> InvalidSizeFiles = new List<string>();
        public List<string> InvalidTypeFiles = new List<string>();
        public List<string> InvalidCountFiles = new List<string>();
        public List<Photo> Photos = new List<Photo>();
    }
}
