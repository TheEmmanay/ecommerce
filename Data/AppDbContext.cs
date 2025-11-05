using Microsoft.EntityFrameworkCore;
using ecommerce.Entities;

namespace ecommerce.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<User> Users { get; set; }

        public DbSet<Product> Products { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
            modelBuilder.Entity<Cart>().HasOne(c => c.User).WithMany();
            modelBuilder.Entity<CartItem>().HasOne(ci => ci.Cart).WithMany(c => c.Items).HasForeignKey(ci => ci.CartId);
            modelBuilder.Entity<CartItem>().HasOne(ci => ci.Product).WithMany();
            base.OnModelCreating(modelBuilder);
        }
    }
}
