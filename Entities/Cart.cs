using System;
using System.Collections.Generic;

namespace ecommerce.Entities
{
    public class Cart
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
        public bool IsConfirmed { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ConfirmedAt { get; set; }

        public ICollection<CartItem>? Items { get; set; }
    }
}
