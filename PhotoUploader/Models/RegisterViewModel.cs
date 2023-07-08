using System.ComponentModel.DataAnnotations;

namespace PhotoUploader.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Укажите логин")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Введите пароль")]
        public string Password { get; set; }
    }
}
