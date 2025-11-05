using ecommerce.Data;
using ecommerce.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ecommerce.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly AppDbContext _ctx;
        public CartRepository(AppDbContext ctx) => _ctx = ctx;

        public async Task AddCartAsync(Cart cart) => await _ctx.Carts.AddAsync(cart);

        public async Task<Cart?> GetActiveCartByUserIdAsync(int userId)
        {
            return await _ctx.Carts.Include(c => c.Items).ThenInclude(i => i.Product).FirstOrDefaultAsync(c => c.UserId == userId && !c.IsConfirmed);
        }

        public async Task<Cart?> GetCartByIdAsync(int cartId)
        {
            return await _ctx.Carts.Include(c => c.Items).ThenInclude(i => i.Product).FirstOrDefaultAsync(c => c.Id == cartId);
        }

        public async Task<IEnumerable<CartItem>> GetItemsByCartIdAsync(int cartId)
        {
            return await _ctx.CartItems.Where(i => i.CartId == cartId).Include(i => i.Product).ToListAsync();
        }

        public async Task<CartItem?> GetItemByCartAndProductAsync(int cartId, int productId)
        {
            return await _ctx.CartItems.FirstOrDefaultAsync(i => i.CartId == cartId && i.ProductId == productId);
        }

        public async Task AddItemAsync(CartItem item) => await _ctx.CartItems.AddAsync(item);

        public async Task UpdateItemAsync(CartItem item)
        {
            _ctx.CartItems.Update(item);
            await Task.CompletedTask;
        }

        public async Task DeleteItemAsync(CartItem item)
        {
            _ctx.CartItems.Remove(item);
            await Task.CompletedTask;
        }

        public async Task SaveChangesAsync() => await _ctx.SaveChangesAsync();

        public async Task ConfirmCartAsync(Cart cart)
        {
            cart.IsConfirmed = true;
            cart.ConfirmedAt = System.DateTime.UtcNow;
            _ctx.Carts.Update(cart);
            await Task.CompletedTask;
        }
    }
}
