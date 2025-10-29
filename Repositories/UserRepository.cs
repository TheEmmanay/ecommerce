using ecommerce.Data;
using ecommerce.Entities;
using Microsoft.EntityFrameworkCore;

namespace ecommerce.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _ctx;
        public UserRepository(AppDbContext ctx) => _ctx = ctx;
        public async Task AddAsync(User user) => await _ctx.Users.AddAsync(user);
        public async Task<User> GetByEmailAsync(string email) => await _ctx.Users.FirstOrDefaultAsync(u => u.Email == email);
        public async Task<User> GetByIdAsync(int id) => await _ctx.Users.FindAsync(id);
        public async Task SaveChangesAsync() => await _ctx.SaveChangesAsync();
    }
}
