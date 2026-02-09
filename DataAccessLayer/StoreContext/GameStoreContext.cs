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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ===================== GameEntity =====================
        modelBuilder.Entity<GameEntity>(entity =>
        {
            entity.HasKey(g => g.Id); // Publisher (один Publisher має багато Games)

            entity.HasOne(g => g.Publisher)
             .WithMany(p => p.Games)
             .HasForeignKey(g => g.PublisherId)
             .OnDelete(DeleteBehavior.Cascade); // Якщо видаляється Publisher → видаляються всі його ігри

            // One-to-One: Game ↔ GameFile
            entity.HasOne(g => g.GameFile)
            .WithOne(f => f.Game)
            .HasForeignKey<GameFileEntity>(f => f.GameId)
            .OnDelete(DeleteBehavior.Cascade); // Якщо видаляється Game → видаляється її файл
        });

        // ===================== GameFileEntity =====================
        modelBuilder.Entity<GameFileEntity>(entity =>
        {
            entity.HasKey(f => f.GameId); // GameId одночасно є PK і FK → класичний one-to-one
        });

        // ===================== GameGenreEntity =====================
        modelBuilder.Entity<GameGenreEntity>(entity =>
        {
            entity.HasKey(gg => new { gg.GameId, gg.GenreId }); // композитний ключ

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
            entity.HasKey(gp => new { gp.GameId, gp.PlatformId }); // композитний ключ

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
            .OnDelete(DeleteBehavior.Cascade); // Якщо видаляється Publisher → видаляються всі його ігри
        });
    }
}