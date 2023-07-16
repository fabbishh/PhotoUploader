using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhotoUploader.Models;
using PhotoUploader.Services;

namespace PhotoUploader.Controllers
{
    [Authorize]
    public class PhotoController : Controller
    {
        private readonly IPhotoService _photoService;
        private const int _maxFiles = 10;

        public PhotoController(IPhotoService photoService)
        {
            _photoService = photoService;
        }

        [HttpGet]
        public IActionResult Index() => View();

        [HttpGet]
        public async Task<IActionResult> UploadPopup()
        {
            var photos = await _photoService.GetPhotosAsync();
            return PartialView("_UploadPopup", photos);
        }

        [HttpPost]
        [RequestSizeLimit(long.MaxValue)]
        public async Task<IActionResult> Upload(UploadPhotosModel model)
        {
            if (model.Files == null)
            {
                return PartialView("_PhotoListPartial", await _photoService.GetPhotosAsync());
            }

            var response = await _photoService.UploadPhotoAsync(new UploadPhotosModel { Files = model.Files, TagId = model.TagId }, _maxFiles);

            var photos = await _photoService.GetPhotosAsync();

            if ((response.InvalidCountFiles.Count + response.InvalidTypeFiles.Count + response.InvalidSizeFiles.Count + response.DuplicateFiles.Count) == 0)
            {
                ViewBag.Success = true;
            }
            else
            {
                ViewBag.InvalidSizeFiles = response.InvalidSizeFiles;
                ViewBag.InvalidTypeFiles = response.InvalidTypeFiles;
                ViewBag.InvalidCountFiles = response.InvalidCountFiles;
                ViewBag.FilesCount = model.Files.Count;
                ViewBag.UploadedFilesCount = response.ValidFiles.Count;
                ViewBag.DuplicateFiles = response.DuplicateFiles;
            }

            return PartialView("_PhotoListPartial", photos);
        }

        [HttpPost]
        public async Task<IActionResult> SetMain(Guid id)
        {
            // Установка выбранной фото как главной в базе данных
            await _photoService.SetMainPhotoAsync(id);

            // Обновление списка фото на странице (частичное представление)
            var photos = await _photoService.GetPhotosAsync();
            return PartialView("_PhotoListPartial", photos);
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(Guid id)
        {
            // Удаление фото из базы данных и файлов с сервера
            await _photoService.DeletePhotoAsync(id);

            // Обновление списка фото на странице (частичное представление)
            var photos = await _photoService.GetPhotosAsync();
            return PartialView("_PhotoListPartial", photos);
        }

        [HttpGet]
        public async Task<IActionResult> Gallery()
        {
            var photos = await _photoService.GetPhotosAsync();
            List<string> tags = photos
                .Select(photo => photo.Tag?.Name)
                .Distinct()
                .ToList();

            if(photos.Any(p => p.Tag == null))
            {
                tags.Insert(0, "Без Тега");
            }
            
            var photosViewModel = new List<PhotoViewModel>();
            foreach(var photo in photos)
            {
                photosViewModel.Add(new PhotoViewModel
                {
                    Id = photo.Id,
                    FileName = photo.FileName,
                    UrlOriginal = photo.UrlOriginal,
                    UrlSmall = photo.UrlSmall,
                    UrlThumb = photo.UrlThumb,
                    IsMain = photo.IsMain,
                    TagName = photo.Tag == null ? "Без Тега" : photo.Tag.Name
                });
            }

            GalleryViewModel model = new GalleryViewModel
            {
                Photos = photosViewModel,
                Tags = tags
            };

            return View(model);
        }
    }
}
