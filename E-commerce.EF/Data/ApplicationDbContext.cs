using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using E_commerce.Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;


namespace E_commerce.EF.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser,ApplicationRole,int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<ShoppingCart> ShoppingCarts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<ApplicationUser>()
            .HasOne(u => u.ShoppingCart)
            .WithOne(c => c.User)
            .HasForeignKey<ShoppingCart>(c => c.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ApplicationUser>()
            .HasMany(u => u.Orders).WithOne(o => o.User).HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Category>()
            .HasMany(c => c.Products).WithOne(p => p.Category).HasForeignKey(p => p.CategoryId);

            builder.Entity<Product>()
            .HasMany(c=>c.OrderItems).WithOne(oi => oi.Product).HasForeignKey(oi => oi.ProductId);

            builder.Entity<Product>()   
            .HasMany(c => c.CartItems).WithOne(ci => ci.Product).HasForeignKey(ci => ci.ProductId);

            builder.Entity<ShoppingCart>()
            .HasMany(c => c.CartItems).WithOne(ci => ci.ShoppingCart).HasForeignKey(ci => ci.ShoppingCartId);

            builder.Entity<Order>()
           .HasMany(o => o.OrderItems).WithOne(oi => oi.Order).HasForeignKey(oi => oi.OrderId);


        }
    }
}
