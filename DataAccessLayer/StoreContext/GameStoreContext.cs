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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<GameEntity>().HasIndex(g => g.GameKey).IsUnique();

        modelBuilder.Entity<GenreEntity>()
            .HasMany(g => g.SubGenres)
            .WithOne(g => g.ParentGenre)
            .HasForeignKey(g => g.ParentGenreId);

        modelBuilder.Entity<PlatformEntity>().HasIndex(p => p.PlatformType).IsUnique();

        modelBuilder.Entity<GameGenreEntity>().HasKey(gg => new { gg.GameId, gg.GenreId });
        modelBuilder.Entity<GamePlatformEntity>().HasKey(gp => new { gp.GameId, gp.PlatformId });

        modelBuilder.Entity<GameGenreEntity>()
            .HasOne(gg => gg.Game)
            .WithMany(g => g.GameGenres)
            .HasForeignKey(gg => gg.GameId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<GameGenreEntity>()
            .HasOne(gg => gg.Genre)
            .WithMany()
            .HasForeignKey(gg => gg.GenreId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<GamePlatformEntity>()
            .HasOne(gp => gp.Game)
            .WithMany(g => g.GamePlatforms)
            .HasForeignKey(gp => gp.GameId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<GamePlatformEntity>()
            .HasOne(gp => gp.Platform)
            .WithMany()
            .HasForeignKey(gp => gp.PlatformId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
