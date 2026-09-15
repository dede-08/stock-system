using Microsoft.EntityFrameworkCore;
using api_gestion_productos.Models;

namespace api_gestion_productos.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>(e =>
            {
                e.Property(p => p.name).HasMaxLength(100).IsRequired();
                e.Property(p => p.description).HasMaxLength(500);
                e.Property(p => p.price).HasPrecision(10, 2);
                e.Property(p => p.category).HasMaxLength(50).IsRequired();
                e.Property(p => p.createdAt).HasDefaultValueSql("NOW()");
                e.HasQueryFilter(p => p.isActive);
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
                e.Property(u => u.password).HasMaxLength(200).IsRequired();
                e.Property(u => u.role).HasMaxLength(20).HasDefaultValue("USER");
                e.HasQueryFilter(u => u.isActive);
                e.HasIndex(u => u.email).IsUnique();
                e.HasIndex(u => u.isActive);
            });

            modelBuilder.Entity<RefreshToken>(e =>
            {
                e.Property(r => r.tokenHash).HasMaxLength(64).IsRequired();
                e.Property(r => r.replacedByTokenHash).HasMaxLength(64);
                e.HasIndex(r => r.tokenHash).IsUnique();
                e.HasIndex(r => r.userId);
                e.HasIndex(r => r.expiresAt);
                e.HasOne(r => r.user)
                    .WithMany()
                    .HasForeignKey(r => r.userId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }

        public override Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.Entity is Product p)
                {
                    if (entry.State == EntityState.Added)
                    {
                        p.createdAt = DateTime.UtcNow;
                        p.isActive = true;
                    }
                    else if (entry.State == EntityState.Modified)
                    {
                        p.updatedAt = DateTime.UtcNow;
                    }
                }
                else if (entry.Entity is User u)
                {
                    if (entry.State == EntityState.Added)
                    {
                        u.createdat = DateTime.UtcNow;
                        u.isActive = true;
                    }
                    else if (entry.State == EntityState.Modified)
                    {
                        u.updatedat = DateTime.UtcNow;
                    }
                }
                else if (entry.Entity is RefreshToken r)
                {
                    if (entry.State == EntityState.Added)
                        r.createdAt = DateTime.UtcNow;
                }
            }
            return base.SaveChangesAsync(ct);
        }
    }
}
