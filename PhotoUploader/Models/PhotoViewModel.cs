namespace PhotoUploader.Models
{
    public class PhotoViewModel
    {
        public Guid Id { get; set; }
        public string FileName { get; set; }
        public string UrlOriginal { get; set; }
        public string UrlSmall { get; set; }
        public string UrlThumb { get; set; }
        public bool IsMain { get; set; }
        public string TagName { get; set; }
    }
}
