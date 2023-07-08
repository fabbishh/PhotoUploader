using PhotoUploader.Entities;
using PhotoUploader.Helpers;
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

        public async Task UploadPhotoAsync(List<IFormFile> files)
        {
            var hasPhoto = (await _photoRepository.GetUserPhotosAsync(_userId)).Any();
            foreach(var file in files)
            {
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
                    UserId = _userId,
                    DateCreated = DateTimeOffset.UtcNow
                };

                // Сохраняем информацию о фото в базе данных
                await _photoRepository.SavePhotoAsync(photo);

                if (!hasPhoto)
                {
                    hasPhoto = true;
                }
            }
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

            string filePathForDb = Path.Combine($"Files\\User{_userId}\\", type, fileName);

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

            // Удаляем файлы с сервера
            File.Delete(Path.Combine(_env.WebRootPath, photo.UrlOriginal));
            File.Delete(Path.Combine(_env.WebRootPath, photo.UrlSmall));
            File.Delete(Path.Combine(_env.WebRootPath, photo.UrlThumb));
        }

        public bool IsSupportedFileType(string fileName)
        {
            var allowedExtensions = new[] { ".jpeg", ".jpg", ".png" };
            var fileExtension = Path.GetExtension(fileName).ToLowerInvariant();
            return allowedExtensions.Contains(fileExtension);
        }

        private void ResizeImage(IFormFile file, string url, int width, int height)
        {
            using (Image image = Image.Load(file.OpenReadStream()))
            {
                // Изменяем размер и сохраняем изображение размером 100x100
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
