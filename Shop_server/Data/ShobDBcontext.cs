using Microsoft.EntityFrameworkCore;
using Shop_server.Models;

namespace Shop_server.Data
{
    public class ShopDbContext : DbContext
    {
        public ShopDbContext(DbContextOptions<ShopDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Cart> Carts { get; set; } = null!;
        public DbSet<CartItem> CartItems { get; set; } = null!;
        public DbSet<Department> Departments { get; set; } = null!;
        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<OrderItem> OrderItems { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Order>()
                .ToTable("Orders");

            // User 1 — 0..1 Cart:
            // Delete user -> delete chart
            modelBuilder.Entity<User>()
                .HasOne(user => user.Cart)
                .WithOne(cart => cart.User)
                .HasForeignKey<Cart>(cart => cart.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(user => user.Username)
                    .IsUnique()
                    .HasDatabaseName("UX_Users_Username");

                entity.HasIndex(user => user.Email)
                    .IsUnique()
                    .HasDatabaseName("UX_Users_Email");

                entity.ToTable(tableBuilder =>
                    tableBuilder.HasCheckConstraint(
                        "CK_Users_Role",
                        "[Role] IN (0, 1, 2)"));
            });


            // User 1 — N Orders:
            // Delete user -set null for order
            modelBuilder.Entity<Order>()
                .HasOne(order => order.User)
                .WithMany(user => user.Orders)
                .HasForeignKey(order => order.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Order>()
                .ToTable("Orders", tableBuilder =>
                {
                    tableBuilder.HasCheckConstraint(
                        "CK_Orders_TotalPrice",
                        "[TotalPrice] >= 0");

                    tableBuilder.HasCheckConstraint(
                        "CK_Orders_Status",
                        "[Status] IN (0, 1, 2, 3)");
                });

            // Department 1 — N Products:
            // Delete Department - set null for product
            modelBuilder.Entity<Product>()
                .HasOne(product => product.Department)
                .WithMany(department => department.Products)
                .HasForeignKey(product => product.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable(tableBuilder =>
                {
                    tableBuilder.HasCheckConstraint(
                        "CK_Products_Price",
                        "[Price] >= 0");

                    tableBuilder.HasCheckConstraint(
                        "CK_Products_Stock",
                        "[Stock] >= 0");
                });
            });

            // Delete ChartItem - element in Chart has removed
            modelBuilder.Entity<CartItem>()
                .HasKey(item => item.Id);

            modelBuilder.Entity<CartItem>()
                .HasIndex(item => new
                {
                    item.CartId,
                    item.ProductId
                })
                .IsUnique();

            modelBuilder.Entity<CartItem>()
                .Property(item => item.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<CartItem>()
                .HasIndex(item => new { item.CartId, item.ProductId })
                .IsUnique();

            // Delete Chart - delete chart items
            modelBuilder.Entity<CartItem>()
                .HasOne(item => item.Cart)
                .WithMany(cart => cart.Items)
                .HasForeignKey(item => item.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            // Delete Product -> Delete chartitem
            modelBuilder.Entity<CartItem>()
                .HasOne(item => item.Product)
                .WithMany(product => product.CartItems)
                .HasForeignKey(item => item.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CartItem>()
                .ToTable(tableBuilder =>
                {
                     tableBuilder.HasCheckConstraint(
                        "CK_CartItems_Quantity",
                        "[Quantity] >= 1");

                     tableBuilder.HasCheckConstraint(
                        "CK_CartItems_UnitPrice",
                        "[UnitPrice] >= 0");
                });

            // OrderItem has usual PK Id
            modelBuilder.Entity<OrderItem>()
                .HasKey(item => item.Id);

            // Delet Order -> Delete OrderItems
            modelBuilder.Entity<OrderItem>()
                .HasOne(item => item.Order)
                .WithMany(order => order.Items)
                .HasForeignKey(item => item.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Delete Product - set Null for OrderItem
            modelBuilder.Entity<OrderItem>()
                .HasOne(item => item.Product)
                .WithMany(product => product.OrderItems)
                .HasForeignKey(item => item.ProductId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<User>()
                .HasIndex(user => user.Username)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(user => user.Email)
                .IsUnique();

            modelBuilder.Entity<Product>()
                .Property(product => product.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Product>()
               .HasIndex(product => product.Name)
               .IsUnique();

            modelBuilder.Entity<Department>()
               .HasIndex(dep => dep.Name)
               .IsUnique();


            modelBuilder.Entity<CartItem>()
                .Property(item => item.UnitPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<OrderItem>()
                .Property(item => item.UnitPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Cart>()
                .Property(cart => cart.TotalPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Order>()
                .Property(order => order.TotalPrice)
                .HasPrecision(18, 2);
        }
    }
    
}
