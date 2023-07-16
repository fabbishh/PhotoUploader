using PhotoUploader.Entities;
using PhotoUploader.Helpers;
using PhotoUploader.Models;
using PhotoUploader.Repository;
using System.Security.Claims;

namespace PhotoUploader.Services
{
    public class PhotoService : IPhotoService
    {
        private readonly IPhotoRepository _photoRepository;
        private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment _env;
        private readonly Guid _userId;

        public PhotoService(IPhotoRepository photoRepository, 
            Microsoft.AspNetCore.Hosting.IHostingEnvironment env, 
            IHttpContextAccessor httpContextAccessor)
        {
            _photoRepository = photoRepository;
            _env = env;
            _userId = Guid.Parse(httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
        }

        public async Task<List<Photo>> GetPhotosAsync()
        {
            return await _photoRepository.GetUserPhotosAsync(_userId);
        }

        public async Task<FilesResponseModel> UploadPhotoAsync(UploadPhotosModel model, int maxFiles)
        {
            var existingPhotos = await _photoRepository.GetUserPhotosAsync(_userId);
            var hasPhoto = existingPhotos.Any();
            var response = new FilesResponseModel();

            var totalFileCount = existingPhotos.Count;

            foreach (var file in model.Files)
            {
                //Проверка лимита на загрузку файлов
                if (totalFileCount >= maxFiles)
                {
                    response.InvalidCountFiles.Add(file.FileName);
                    continue;
                }
                //Проверка размера файла
                if (file.Length < 2 * 1024 * 1024 || file.Length > 8 * 1024 * 1024)
                {
                    response.InvalidSizeFiles.Add(file.FileName);
                    continue;
                }
                //Проверка типа файла
                if (!IsSupportedFileType(file.FileName))
                {
                    response.InvalidTypeFiles.Add(file.FileName);
                    continue;
                }

                byte[] photoBytes;
                using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);
                    photoBytes = memoryStream.ToArray();
                }

                string photoHash = EncryptionHelper.ComputePhotoHash(photoBytes);

                //Проверка на дубликат
                bool photoExists = await _photoRepository.CheckPhotoExistsAsync(photoHash, _userId);
                if (photoExists)
                {
                    response.DuplicateFiles.Add(file.FileName);
                    continue;
                }

                // Генерируем уникальное имя файла
                var photoGuid = Guid.NewGuid();

                var originalUrl = SavePhotoToStorage(photoGuid, ImageFormat.Original, file);
                var thumbUrl = SavePhotoToStorage(photoGuid, ImageFormat.Thumb, file);
                var smallUrl = SavePhotoToStorage(photoGuid, ImageFormat.Small, file);

                // Создаем объект модели Photo
                Photo photo = new Photo
                {
                    Id = photoGuid,
                    FileName = photoGuid.ToString(),
                    UrlOriginal = originalUrl,
                    UrlSmall = smallUrl,
                    UrlThumb = thumbUrl,
                    IsMain = !hasPhoto,
                    Hash = photoHash,
                    TagId = model.TagId,
                    UserId = _userId,
                    DateCreated = DateTimeOffset.UtcNow
                };

                // Сохраняем информацию о фото в базе данных
                await _photoRepository.SavePhotoAsync(photo);
                response.ValidFiles.Add(file.FileName);
                totalFileCount++;

                if (!hasPhoto)
                {
                    hasPhoto = true;
                }
            }

            return response;
        }

        private string SavePhotoToStorage(Guid fileId, string type, IFormFile file)
        {
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();

            string fileName = $"{fileId.ToString()}-{type}{ext}".ToLowerInvariant();
            
            string uploadPath = Path.Combine(_env.WebRootPath, $"Files\\User{_userId}\\", type);

            // Создаем директорию, если она не существует
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            string filePath = Path.Combine(uploadPath, fileName);

            if (type == ImageFormat.Original)
            {
                using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(fileStream);
                }
            }
            else if (type == ImageFormat.Thumb)
            {
                ResizeImage(file, filePath, 600, 400);
            }
            else if (type == ImageFormat.Small)
            {
                ResizeImage(file, filePath, 100, 100);
            }
            else
            {
                throw new InvalidDataException("format type is not supported");
            }

            string filePathForDb = Path.Combine($"\\Files\\User{_userId}\\", type, fileName);

            return filePathForDb;
        }

        public async Task SetMainPhotoAsync(Guid id)
        {
            // Получаем фото по идентификатору
            Photo photo = await _photoRepository.GetPhotoByIdAsync(id);
            if (photo == null)
            {
                // Обработка случая, когда фото не найдено
                return;
            }

            // Снимаем признак главной фото со всех фото
            await _photoRepository.ClearMainPhotoAsync(_userId);

            // Устанавливаем выбранное фото как главное
            photo.IsMain = true;
            await _photoRepository.SavePhotoAsync(photo);
        }

        public async Task DeletePhotoAsync(Guid id)
        {
            // Получаем фото по идентификатору
            Photo photo = await _photoRepository.GetPhotoByIdAsync(id);
            if (photo == null)
            {
                // Обработка случая, когда фото не найдено
                return;
            }

            // Удаляем информацию о фото из базы данных
            await _photoRepository.DeletePhotoAsync(id);

            //Если удаляем главное фото, устанавливаем новое главное
            if (photo.IsMain)
            {
                var photos = await _photoRepository.GetUserPhotosAsync(_userId);
                if (photos.Count > 0)
                {
                    var newMainPhoto = photos.First();
                    newMainPhoto.IsMain = true;
                    await _photoRepository.SavePhotoAsync(newMainPhoto);
                }
            }

            // Удаляем файлы с сервера
            File.Delete(Path.Combine(_env.WebRootPath, photo.UrlOriginal.Remove(0, 1)));
            File.Delete(Path.Combine(_env.WebRootPath, photo.UrlSmall.Remove(0, 1)));
            File.Delete(Path.Combine(_env.WebRootPath, photo.UrlThumb.Remove(0, 1)));
        }

        private bool IsSupportedFileType(string fileName)
        {
            var allowedExtensions = new[] { ".jpeg", ".jpg", ".png" };
            var fileExtension = Path.GetExtension(fileName).ToLowerInvariant();
            return allowedExtensions.Contains(fileExtension);
        }

        private void ResizeImage(IFormFile file, string url, int width, int height)
        {
            using (Image image = Image.Load(file.OpenReadStream()))
            {
                // Изменяем размер и сохраняем изображение с новым размером
                using (Image resizedImage = image.Clone(x => x.Resize(new ResizeOptions
                {
                    Size = new Size(width, height),
                    Mode = ResizeMode.Max
                })))
                {
                    resizedImage.Save(url);
                }
            }
        }
    }
}
