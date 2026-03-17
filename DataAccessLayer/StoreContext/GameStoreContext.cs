using DataAccessLayer.Entities;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.StoreContext;

public class GameStoreContext(DbContextOptions<GameStoreContext> options) : DbContext(options)
{
    public DbSet<GameEntity> Games { get; set; }

    public DbSet<GameGenreEntity> GameGenres { get; set; }

    public DbSet<GamePlatformEntity> GamePlatforms { get; set; }

    public DbSet<GenreEntity> Genres { get; set; }

    public DbSet<PlatformEntity> Platforms { get; set; }

    public DbSet<GameFileEntity> Files { get; set; }

    public DbSet<PublisherEntity> Publishers { get; set; }

    public DbSet<OrderEntity> Orders { get; set; }

    public DbSet<OrderGameEntity> OrderGames { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ===================== GameEntity =====================
        modelBuilder.Entity<GameEntity>(entity =>
        {
            entity.HasKey(g => g.Id);

            entity.HasOne(g => g.Publisher)
             .WithMany(p => p.Games)
             .HasForeignKey(g => g.PublisherId)
             .OnDelete(DeleteBehavior.Cascade);

            // One-to-One: Game ↔ GameFile
            entity.HasOne(g => g.GameFile)
            .WithOne(f => f.Game)
            .HasForeignKey<GameFileEntity>(f => f.GameId)
            .OnDelete(DeleteBehavior.Cascade);
        });

        // ===================== GameFileEntity =====================
        modelBuilder.Entity<GameFileEntity>(entity =>
        {
            entity.HasKey(f => f.GameId);
        });

        // ===================== GameGenreEntity =====================
        modelBuilder.Entity<GameGenreEntity>(entity =>
        {
            entity.HasKey(gg => new { gg.GameId, gg.GenreId });

            entity.HasOne(gg => gg.Game)
            .WithMany(g => g.GameGenres)
            .HasForeignKey(gg => gg.GameId)
            .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(gg => gg.Genre)
            .WithMany()
            .HasForeignKey(gg => gg.GenreId)
            .OnDelete(DeleteBehavior.Cascade);
        });

        // ===================== GamePlatformEntity =====================
        modelBuilder.Entity<GamePlatformEntity>(entity =>
        {
            entity.HasKey(gp => new { gp.GameId, gp.PlatformId });

            entity.HasOne(gp => gp.Game)
            .WithMany(g => g.GamePlatforms)
            .HasForeignKey(gp => gp.GameId)
            .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(gp => gp.Platform).WithMany().HasForeignKey(gp => gp.PlatformId).OnDelete(DeleteBehavior.Cascade);
        });

        // ===================== GenreEntity =====================
        modelBuilder.Entity<GenreEntity>(entity =>
        {
            entity.HasKey(g => g.Id);                            // Self-reference: ParentGenre → SubGenres

            entity.HasOne(g => g.ParentGenre)
            .WithMany(g => g.SubGenres)
            .HasForeignKey(g => g.ParentGenreId);
        });

        // ===================== PlatformEntity =====================
        modelBuilder.Entity<PlatformEntity>().HasIndex(p => p.Type).IsUnique();

        // ===================== PublisherEntity =====================
        modelBuilder.Entity<PublisherEntity>(entity =>
        {
            entity.HasKey(p => p.Id);

            entity.HasMany(p => p.Games)
            .WithOne(g => g.Publisher)
            .HasForeignKey(g => g.PublisherId)
            .OnDelete(DeleteBehavior.Cascade);
        });

        // ===================== GenreEntity =====================
        modelBuilder.Entity<GenreEntity>(entity =>
        {
            entity.HasKey(g => g.Id);                            // Self-reference: ParentGenre → SubGenres

            entity.HasOne(g => g.ParentGenre)
            .WithMany(g => g.SubGenres)
            .HasForeignKey(g => g.ParentGenreId);
        });

        // ===================== PlatformEntity =====================
        modelBuilder.Entity<PlatformEntity>().HasIndex(p => p.Type).IsUnique();

        // ===================== PublisherEntity =====================
        modelBuilder.Entity<PublisherEntity>(entity =>
        {
            entity.HasKey(p => p.Id);

            entity.HasMany(p => p.Games)
            .WithOne(g => g.Publisher)
            .HasForeignKey(g => g.PublisherId)
            .OnDelete(DeleteBehavior.Cascade);
        });

        // ===================== OrderEntity =====================
        modelBuilder.Entity<OrderEntity>(entity =>
        {
            entity.HasKey(o => o.Id);

            entity.HasMany(o => o.OrderGames)
                .WithOne(og => og.Order)
                .HasForeignKey(og => og.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ===================== OrderGameEntity =====================
        modelBuilder.Entity<OrderGameEntity>(entity =>
        {
            entity.HasKey(og => new { og.OrderId, og.ProductId });

            entity.HasOne(og => og.Order)
                .WithMany(o => o.OrderGames)
                .HasForeignKey(og => og.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(og => og.Game)
                .WithMany()
                .HasForeignKey(og => og.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}