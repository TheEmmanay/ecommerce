using ecommerce.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ecommerce.Services
{
    public interface IUserService
    {
        Task<IEnumerable<User>> GetAllUsers();
        Task<User> GetUserById(int id);
        Task<User> CreateUser(User user);
        Task UpdateUser(User user, bool changePassword);
        Task DeleteUser(int id);
    }
}
