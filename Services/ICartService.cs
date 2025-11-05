using ecommerce.DTOs;
using System.Threading.Tasks;

namespace ecommerce.Services
{
    public interface ICartService
    {
        Task<CartResponse?> GetCartForUserAsync(int userId);
        Task<CartResponse?> AddProductToCartAsync(int userId, AddToCartRequest request);
        Task<bool> RemoveItemAsync(int userId, int itemId);
        Task<CartResponse?> UpdateItemQuantityAsync(int userId, UpdateCartItemRequest request);
        Task<CartResponse?> ConfirmCartAsync(int userId);
    }
}
