using ecommerce.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ecommerce.Repositories
{
    public interface ICartRepository
    {
        Task<Cart?> GetActiveCartByUserIdAsync(int userId);
        Task<Cart?> GetCartByIdAsync(int cartId);
        Task<IEnumerable<CartItem>> GetItemsByCartIdAsync(int cartId);
        Task<CartItem?> GetItemByCartAndProductAsync(int cartId, int productId);
        Task AddCartAsync(Cart cart);
        Task AddItemAsync(CartItem item);
        Task UpdateItemAsync(CartItem item);
        Task DeleteItemAsync(CartItem item);
        Task SaveChangesAsync();
        Task ConfirmCartAsync(Cart cart);
    }
}
