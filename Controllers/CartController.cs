using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ecommerce.Services;
using ecommerce.DTOs;
using System.Security.Claims;

namespace ecommerce.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        private int GetUserId()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(idClaim, out var id)) return id;
            throw new System.ApplicationException("Invalid user id in token");
        }

        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            var userId = GetUserId();
            var cart = await _cartService.GetCartForUserAsync(userId);
            if (cart == null) return NotFound(new { message = "No active cart" });
            return Ok(cart);
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
        {
            var userId = GetUserId();
            try
            {
                var cart = await _cartService.AddProductToCartAsync(userId, request);
                return Ok(cart);
            }
            catch (System.ApplicationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("item/{itemId}")]
        public async Task<IActionResult> RemoveItem(int itemId)
        {
            var userId = GetUserId();
            var ok = await _cartService.RemoveItemAsync(userId, itemId);
            if (!ok) return NotFound();
            return NoContent();
        }

        [HttpPut("item")]
        public async Task<IActionResult> UpdateItem([FromBody] UpdateCartItemRequest request)
        {
            var userId = GetUserId();
            var cart = await _cartService.UpdateItemQuantityAsync(userId, request);
            if (cart == null) return NotFound();
            return Ok(cart);
        }

        [HttpPost("confirm")]
        public async Task<IActionResult> Confirm()
        {
            var userId = GetUserId();
            var cart = await _cartService.ConfirmCartAsync(userId);
            if (cart == null) return NotFound();
            return Ok(cart);
        }
    }
}
