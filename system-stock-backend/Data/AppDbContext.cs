using Microsoft.EntityFrameworkCore;
using api_gestion_productos.Models;

namespace api_gestion_productos.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>(e =>
            {
                e.Property(p => p.name).HasMaxLength(100).IsRequired();
                e.Property(p => p.description).HasMaxLength(500);
                e.Property(p => p.price).HasPrecision(10, 2);
                e.Property(p => p.category).HasMaxLength(50).IsRequired();
                e.HasIndex(p => p.isActive);
                e.HasIndex(p => p.category);
                e.HasIndex(p => p.name);
                e.HasIndex(p => p.stock);
            });

            modelBuilder.Entity<User>(e =>
            {
                e.Property(u => u.name).HasMaxLength(50).IsRequired();
                e.Property(u => u.lastname).HasMaxLength(50).IsRequired();
                e.Property(u => u.email).HasMaxLength(100).IsRequired();
                e.HasIndex(u => u.email).IsUnique();
                e.HasIndex(u => u.isActive);
            });
        }
    }
}
