using System;
using System.Collections.Generic;

namespace ecommerce.DTOs
{
    public record AddToCartRequest(int ProductId, int Quantity);
    public record UpdateCartItemRequest(int ItemId, int Quantity);
    public record CartItemResponse(int Id, int ProductId, string? ProductName, int UnitPrice, int Quantity);
    public record CartResponse(int CartId, int UserId, bool IsConfirmed, DateTime CreatedAt, DateTime? ConfirmedAt, IEnumerable<CartItemResponse> Items);
}
