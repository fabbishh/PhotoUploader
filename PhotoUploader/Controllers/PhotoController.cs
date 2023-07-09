using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhotoUploader.Services;

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
    public async Task<IActionResult> Upload(List<IFormFile> files)
    {
        if(files == null)
        {
            return PartialView("_PhotoListPartial", await _photoService.GetPhotosAsync());
        }

        var invalidSizeFiles = new List<string>();
        var invalidTypeFiles = new List<string>();
        var invalidCountFiles = new List<string>();
        var validFiles = new List<IFormFile>();

        var existingPhotos = await _photoService.GetPhotosAsync();
        var totalFileCount = existingPhotos.Count;

        foreach (var file in files)
        {
            //Проверка лимита на загрузку файлов
            if (totalFileCount >= _maxFiles)
            {
                invalidCountFiles.Add(file.FileName);
                continue;
            }
            //Проверка размера файла
            if (file.Length < 2 * 1024 * 1024 || file.Length > 8 * 1024 * 1024)
            {
                invalidSizeFiles.Add(file.FileName);
                continue;
            }
            //Проверка типа файла
            if (!_photoService.IsSupportedFileType(file.FileName))
            {
                invalidTypeFiles.Add(file.FileName);
                continue;
            }

            validFiles.Add(file);
            totalFileCount++;
        }

        await _photoService.UploadPhotoAsync(validFiles);

        var photos = await _photoService.GetPhotosAsync();

        

        if((invalidSizeFiles.Count + invalidTypeFiles.Count + invalidCountFiles.Count) == 0)
        {
            ViewBag.Success = true;
        } else
        {
            ViewBag.InvalidSizeFiles = invalidSizeFiles;
            ViewBag.InvalidTypeFiles = invalidTypeFiles;
            ViewBag.InvalidCountFiles = invalidCountFiles;
            ViewBag.FilesCount = files.Count;
            ViewBag.UploadedFilesCount = validFiles.Count;
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
}
