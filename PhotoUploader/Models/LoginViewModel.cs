using System.ComponentModel.DataAnnotations;

namespace PhotoUploader.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Укажите логин")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Введите пароль")]
        public string Password { get; set; }
    }
}
