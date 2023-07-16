namespace PhotoUploader.Models
{
    public class UploadPhotosModel
    {
        public Guid? TagId { get; set; }
        public List<IFormFile> Files { get; set; }
    }
}
