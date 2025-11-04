using ecommerce.Entities;
using ecommerce.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ecommerce.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repo;

        public UserService(IUserRepository repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<User>> GetAllUsers() => await _repo.GetAllAsync();
        public async Task<User> GetUserById(int id) => await _repo.GetByIdAsync(id);

        public async Task<User> CreateUser(User user)
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
            await _repo.AddAsync(user);
            await _repo.SaveChangesAsync();
            return user;
        }

        public async Task UpdateUser(User user, bool changePassword)
        {
            if (changePassword)
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
            }else
            {
                var existingUser = await _repo.GetByIdAsync(user.Id);
                if (existingUser != null)
                {
                    user.PasswordHash = existingUser.PasswordHash;
                }
            }
            await _repo.UpdateAsync(user);
            await _repo.SaveChangesAsync();
        }

        public async Task DeleteUser(int id)
        {
            await _repo.DeleteAsync(id);
            await _repo.SaveChangesAsync();
        }
    }
}
