using ecommerce.DTOs;
using ecommerce.Entities;
using ecommerce.Repositories;
using ecommerce.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ecommerce.Services
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepo;
        private readonly IUserRepository _userRepo;
        private readonly AppDbContext? _ctx;

        public CartService(ICartRepository cartRepo, IUserRepository userRepo, ecommerce.Data.AppDbContext ctx)
        {
            _cartRepo = cartRepo;
            _userRepo = userRepo;
            _ctx = ctx;
        }

        public async Task<CartResponse?> GetCartForUserAsync(int userId)
        {
            var cart = await _cartRepo.GetActiveCartByUserIdAsync(userId);
            if (cart == null) return null;
            var items = cart.Items?.Select(i => new CartItemResponse(i.Id, i.ProductId, i.Product?.Name, i.UnitPrice, i.Quantity)) ?? Enumerable.Empty<CartItemResponse>();
            return new CartResponse(cart.Id, cart.UserId, cart.IsConfirmed, cart.CreatedAt, cart.ConfirmedAt, items);
        }

        public async Task<CartResponse?> AddProductToCartAsync(int userId, AddToCartRequest request)
        {
            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null) throw new System.ApplicationException("User not found");

            var cart = await _cartRepo.GetActiveCartByUserIdAsync(userId);
            if (cart == null)
            {
                cart = new Cart { UserId = userId, CreatedAt = System.DateTime.UtcNow, IsConfirmed = false };
                await _cartRepo.AddCartAsync(cart);
                await _cartRepo.SaveChangesAsync();
                // reload with id
                cart = await _cartRepo.GetActiveCartByUserIdAsync(userId) ?? cart;
            }

            var prod = await _ctx!.Products.FindAsync(request.ProductId);
            if (prod == null) throw new System.ApplicationException("Product not found");

            var existing = await _cartRepo.GetItemByCartAndProductAsync(cart.Id, request.ProductId);
            if (existing != null)
            {
                existing.Quantity += request.Quantity;
                await _cartRepo.UpdateItemAsync(existing);
            }
            else
            {
                var item = new CartItem
                {
                    CartId = cart.Id,
                    ProductId = prod.Id,
                    Quantity = request.Quantity,
                    UnitPrice = prod.Price,
                    CreatedAt = System.DateTime.UtcNow
                };
                await _cartRepo.AddItemAsync(item);
            }

            await _cartRepo.SaveChangesAsync();
            return await GetCartForUserAsync(userId);
        }

        public async Task<bool> RemoveItemAsync(int userId, int itemId)
        {
            var cart = await _cartRepo.GetActiveCartByUserIdAsync(userId);
            if (cart == null) return false;
            var item = cart.Items?.FirstOrDefault(i => i.Id == itemId);
            if (item == null) return false;
            await _cartRepo.DeleteItemAsync(item);
            await _cartRepo.SaveChangesAsync();
            return true;
        }

        public async Task<CartResponse?> UpdateItemQuantityAsync(int userId, UpdateCartItemRequest request)
        {
            var cart = await _cartRepo.GetActiveCartByUserIdAsync(userId);
            if (cart == null) return null;
            var item = cart.Items?.FirstOrDefault(i => i.Id == request.ItemId);
            if (item == null) return null;
            if (request.Quantity <= 0)
            {
                await _cartRepo.DeleteItemAsync(item);
            }
            else
            {
                item.Quantity = request.Quantity;
                await _cartRepo.UpdateItemAsync(item);
            }
            await _cartRepo.SaveChangesAsync();
            return await GetCartForUserAsync(userId);
        }

        public async Task<CartResponse?> ConfirmCartAsync(int userId)
        {
            var cart = await _cartRepo.GetActiveCartByUserIdAsync(userId);
            if (cart == null) return null;
            await _cartRepo.ConfirmCartAsync(cart);
            await _cartRepo.SaveChangesAsync();
            return await GetCartForUserAsync(userId);
        }
    }
}
