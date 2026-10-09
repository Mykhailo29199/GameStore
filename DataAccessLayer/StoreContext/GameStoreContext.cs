using DataAccessLayer.Entities.GameStoreSQLEntities;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.StoreContext;

public class GameStoreContext(DbContextOptions<GameStoreContext> options) : DbContext(options)
{
    public DbSet<ProductEntity> Products { get; set; }

    public DbSet<ProductCategoryEntity> ProductCategories { get; set; }

    public DbSet<ProductPlatformEntity> ProductPlatforms { get; set; }

    public DbSet<CategoryEntity> Categories { get; set; }

    public DbSet<PlatformEntity> Platforms { get; set; }

    public DbSet<ProductFileEntity> ProductFiles { get; set; }

    public DbSet<SupplierEntity> Suppliers { get; set; }

    public DbSet<OrderEntity> Orders { get; set; }

    public DbSet<OrderDetailsEntity> OrderDetails { get; set; }

    public DbSet<CommentEntity> Comments { get; set; }

    public DbSet<BanEntity> Bans { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ===================== GameEntity =====================
        modelBuilder.Entity<ProductEntity>(entity =>
        {
            entity.HasKey(g => g.Id);

            entity.Property(g => g.Views)
                .IsRequired()
                .HasDefaultValue(0);

            entity.Property(g => g.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(g => g.Supplier)
                .WithMany(p => p.Products)
                .HasForeignKey(g => g.SupplierID)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(g => g.ProductFile)
                .WithOne(f => f.Product)
                .HasForeignKey<ProductFileEntity>(f => f.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ===================== GameFileEntity =====================
        modelBuilder.Entity<ProductFileEntity>(entity =>
        {
            entity.HasKey(f => f.ProductId);
        });

        // ===================== GameGenreEntity =====================
        modelBuilder.Entity<ProductCategoryEntity>(entity =>
        {
            entity.HasKey(gg => new { gg.ProductId, gg.CategoryId });

            entity.HasOne(gg => gg.Product)
                .WithMany(g => g.ProductCategories)
                .HasForeignKey(gg => gg.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(gg => gg.Category)
                .WithMany()
                .HasForeignKey(gg => gg.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ===================== GamePlatformEntity =====================
        modelBuilder.Entity<ProductPlatformEntity>(entity =>
        {
            entity.HasKey(gp => new { gp.ProductId, gp.PlatformId });

            entity.HasOne(gp => gp.Product)
                .WithMany(g => g.ProductPlatforms)
                .HasForeignKey(gp => gp.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(gp => gp.Platform)
                .WithMany()
                .HasForeignKey(gp => gp.PlatformId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ===================== GenreEntity =====================
        modelBuilder.Entity<CategoryEntity>(entity =>
        {
            entity.HasKey(g => g.Id);

            entity.HasOne(g => g.ParentGenre)
                .WithMany(g => g.SubGenres)
                .HasForeignKey(g => g.ParentCategoryId);
        });

        // ===================== PlatformEntity =====================
        modelBuilder.Entity<PlatformEntity>()
            .HasIndex(p => p.Type)
            .IsUnique();

        // ===================== PublisherEntity =====================
        modelBuilder.Entity<SupplierEntity>(entity =>
        {
            entity.HasKey(p => p.Id);

            // Зв'язок уже описаний в GameEntity, але дублювання тут не завадить для ясності
            entity.HasMany(p => p.Products)
                .WithOne(g => g.Supplier)
                .HasForeignKey(g => g.SupplierID)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ===================== OrderEntity =====================
        modelBuilder.Entity<OrderEntity>(entity =>
        {
            entity.HasKey(o => o.Id);

            entity.HasMany(o => o.OrderDetails)
                .WithOne(og => og.Order)
                .HasForeignKey(og => og.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ===================== OrderGameEntity =====================
        modelBuilder.Entity<OrderDetailsEntity>(entity =>
        {
            entity.HasKey(og => new { og.OrderId, og.ProductId });

            entity.HasOne(og => og.Order)
                .WithMany(o => o.OrderDetails)
                .HasForeignKey(og => og.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(og => og.Product)
                .WithMany()
                .HasForeignKey(og => og.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ===================== CommentEntity =====================
        modelBuilder.Entity<CommentEntity>(entity =>
        {
            entity.HasKey(c => c.Id);

            entity.Property(c => c.Name).IsRequired();
            entity.Property(c => c.Body).IsRequired();
            entity.Property(c => c.ProductId).IsRequired();

            entity.HasOne(c => c.Product)
                .WithMany(g => g.Comments)
                .HasForeignKey(c => c.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<CommentEntity>()
                .WithMany()
                .HasForeignKey(c => c.ParentCommentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ===================== BanEntity =====================
        modelBuilder.Entity<BanEntity>(entity =>
        {
            entity.HasKey(b => b.Id);
            entity.HasIndex(b => b.UserName);
            entity.Property(b => b.UserName).IsRequired();
            entity.Property(b => b.ExpireAt).IsRequired(false);
        });
    }
}