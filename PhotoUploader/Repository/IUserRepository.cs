using PhotoUploader.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoUploader.Repository
{
    public interface IUserRepository
    {
        IQueryable<User> GetAll();
        Task Delete(User entity);
        Task Create(User entity);
        Task<User> Update(User entity);
    }
}
