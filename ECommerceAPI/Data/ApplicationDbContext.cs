
using ECommerceAPI.DTOs;
using ECommerceAPI.Model;
using Microsoft.EntityFrameworkCore;
using static ECommerceAPI.Model.OrderProduct;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<AppCustomer> AppCustomers { get; set; }
    public DbSet<Cart> Carts { get; set; }
    //public DbSet<CartItem> CartItems { get; set; }
    public DbSet<ECommerceAPI.Model.OrderProduct> OrderProduct { get; set; }
    public DbSet<CartItem> CartItems { get; set; }



    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure composite primary key for OrderProduct
        modelBuilder.Entity<OrderProduct>()
            .HasKey(op => new { op.OrderId, op.ProductId });

        modelBuilder.Entity<OrderProduct>()
            .HasOne(op => op.Order)
            .WithMany(o => o.OrderProduct)
            .HasForeignKey(op => op.OrderId);

        modelBuilder.Entity<OrderProduct>()
            .HasOne(op => op.Product)
            .WithMany()
            .HasForeignKey(op => op.ProductId);

        modelBuilder.Entity<CartItem>()
            .Property(ci => ci.Price)
            .HasColumnType("decimal(18,2)");


    }
}
