using PhotoUploader.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace PhotoUploader.Services
{
    public interface IAccountService
    {
        Task<ClaimsIdentity> Register(RegisterViewModel model);
        Task<ClaimsIdentity> Login(LoginViewModel model);

    }
}
