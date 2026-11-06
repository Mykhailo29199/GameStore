using DataAccessLayer.Entities.GameStoreSQLEntities;
using GameStore.Shared.Authorization;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.StoreContext;

public class GameStoreContext(DbContextOptions<GameStoreContext> options) : DbContext(options)
{
    // Money is stored as decimal(18, 2), so prices and totals have no floating-point rounding errors.
    private const int MoneyPrecision = 18;
    private const int MoneyScale = 2;

    // Default administrator created by the migration: login "admin", password "admin".
    private const string DefaultAdminName = "admin";
    private const string DefaultAdminPasswordHash = "100000.R2FtZVN0b3JlU2VlZFNhbHQhIQ==.UWluZQtgKpTE83bjFn0gl3j60jbOT/I1BNmfczCbdFA=";

    private static readonly Guid DefaultAdminId = Guid.Parse("b0000000-0000-0000-0000-000000000001");

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

    public DbSet<UserEntity> Users { get; set; }

    public DbSet<RoleEntity> Roles { get; set; }

    public DbSet<UserRoleEntity> UserRoles { get; set; }

    public DbSet<RolePermissionEntity> RolePermissions { get; set; }

    // Entities from Northwind Legacy
    public DbSet<EmployeeEntity> Employees { get; set; }

    public DbSet<TerritoryEntity> Territories { get; set; }

    public DbSet<RegionEntity> Regions { get; set; }

    public DbSet<EmployeeTerritoryEntity> EmployeeTerritories { get; set; }

    public DbSet<ShipperEntity> Shippers { get; set; }

    // The Northwind int ids kept next to the Guid keys look like foreign keys of another type, so EF Core's warning about them is ignored.
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        optionsBuilder.ConfigureWarnings(w => w.Ignore(
            Microsoft.EntityFrameworkCore.Diagnostics.CoreEventId.IncompatibleMatchingForeignKeyProperties));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ===================== ProductEntity =====================
        modelBuilder.Entity<ProductEntity>(entity =>
        {
            entity.HasKey(p => p.Id);

            entity.Property(p => p.ProductName)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(p => p.Key)
                .IsRequired()
                .HasMaxLength(250);

            entity.Property(p => p.UnitPrice)
                .HasPrecision(MoneyPrecision, MoneyScale)
                .IsRequired();

            entity.Property(p => p.Description)
                .HasMaxLength(3000);

            entity.Property(p => p.Discount)
                .HasColumnType("float")
                .HasDefaultValue(0.0);

            entity.Property(p => p.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // Genres and platforms are linked through explicit join entities.
            entity.HasMany(p => p.ProductCategories)
                .WithOne(gg => gg.Product)
                .HasForeignKey(gg => gg.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(p => p.ProductPlatforms)
                .WithOne(gp => gp.Product)
                .HasForeignKey(gp => gp.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(p => p.ProductFile)
                .WithOne(pf => pf.Product)
                .HasForeignKey<ProductFileEntity>(pf => pf.ProductId);

            entity.HasMany(p => p.Comments)
                .WithOne(c => c.Product)
                .HasForeignKey(c => c.ProductId);

            entity.HasIndex(p => p.Key)
                .IsUnique();

            entity.HasQueryFilter(p => !p.IsDeleted);
        });

        // ===================== ProductFileEntity =====================
        modelBuilder.Entity<ProductFileEntity>(entity =>
        {
            entity.HasKey(f => f.ProductId);
        });

        // ===================== ProductCategoryEntity =====================
        modelBuilder.Entity<ProductCategoryEntity>(entity =>
        {
            entity.HasKey(gg => new { gg.ProductId, gg.CategoryId });

            entity.HasOne(gg => gg.Category)
                .WithMany(c => c.ProductCategories)
                .HasForeignKey(gg => gg.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(gg => gg.Product)
                .WithMany(p => p.ProductCategories)
                .HasForeignKey(gg => gg.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ===================== ProductPlatformEntity =====================
        modelBuilder.Entity<ProductPlatformEntity>(entity =>
        {
            entity.HasKey(gp => new { gp.ProductId, gp.PlatformId });

            entity.HasOne(gp => gp.Product)
                .WithMany(g => g.ProductPlatforms)
                .HasForeignKey(gp => gp.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(gp => gp.Platform)
                .WithMany(p => p.ProductPlatforms)
                .HasForeignKey(gp => gp.PlatformId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ===================== CategoryEntity =====================
        modelBuilder.Entity<CategoryEntity>(entity =>
        {
            entity.ToTable("Categories");
            entity.HasKey(g => g.Id);
            entity.HasQueryFilter(g => !g.IsDeleted);

            entity.HasOne(g => g.ParentGenre)
                .WithMany(g => g.SubGenres)
                .HasForeignKey(g => g.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ===================== PlatformEntity =====================
        modelBuilder.Entity<PlatformEntity>()
            .HasIndex(p => p.Type)
            .IsUnique();

        // ===================== SupplierEntity =====================
        modelBuilder.Entity<SupplierEntity>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.ToTable("Suppliers");
            entity.HasQueryFilter(s => !s.IsDeleted);
        });

        // ===================== OrderEntity =====================
        modelBuilder.Entity<OrderEntity>(entity =>
        {
            entity.HasKey(o => o.Id);

            entity.Property(o => o.Freight)
                .HasPrecision(MoneyPrecision, MoneyScale);

            entity.Property(o => o.Status);

            entity.HasMany(o => o.OrderDetails)
                .WithOne(og => og.Order)
                .HasForeignKey(og => og.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ===================== OrderDetailsEntity =====================
        modelBuilder.Entity<OrderDetailsEntity>(entity =>
        {
            entity.ToTable("OrderDetails");
            entity.HasKey(og => new { og.OrderId, og.ProductId });

            entity.Property(og => og.Id).HasDefaultValueSql("NEWID()");
            entity.HasIndex(og => og.Id).IsUnique();

            entity.Property(og => og.UnitPrice)
                .HasColumnName("Price")
                .HasPrecision(MoneyPrecision, MoneyScale)
                .IsRequired();

            entity.Property(og => og.Discount)
                .HasColumnName("Discount")
                .IsRequired(false)
                .HasDefaultValue(0.0);

            entity.HasOne(od => od.Order)
                .WithMany(o => o.OrderDetails)
                .HasForeignKey(od => od.OrderId);

            entity.HasOne(od => od.Product)
                .WithMany()
                .HasForeignKey(od => od.ProductId);
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

        ConfigureAuthorization(modelBuilder);

        // ===================== EmployeeEntity =====================
        modelBuilder.Entity<EmployeeEntity>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasOne(e => e.Manager)
                .WithMany(e => e.Subordinates)
                .HasForeignKey(e => e.ReportsToId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Title).HasMaxLength(100);
        });

        // ===================== RegionEntity =====================
        modelBuilder.Entity<RegionEntity>(entity =>
        {
            entity.HasKey(r => r.Id);

            entity.Property(r => r.RegionDescription)
                .HasMaxLength(50)
                .IsRequired(false);
        });

        // ===================== ShipperEntity =====================
        modelBuilder.Entity<ShipperEntity>(entity =>
        {
            entity.HasKey(s => s.Id);

            entity.Property(s => s.CompanyName)
                .HasMaxLength(40)
                .IsRequired(false);

            entity.Property(s => s.Phone)
                .HasMaxLength(24)
                .IsRequired(false);
        });

        // ===================== CustomerEntity =====================
        modelBuilder.Entity<CustomerEntity>(entity =>
        {
            entity.HasKey(c => c.Id);

            entity.Property(c => c.CompanyName).HasMaxLength(100).IsRequired(false);
            entity.Property(c => c.ContactName).HasMaxLength(100).IsRequired(false);
            entity.Property(c => c.Field11).HasMaxLength(50).IsRequired(false);
        });

        // ===================== TerritoryEntity =====================
        modelBuilder.Entity<TerritoryEntity>(entity =>
        {
            entity.HasKey(t => t.Id);

            entity.Property(t => t.TerritoryDescription)
                .IsRequired()
                .HasMaxLength(100);

            entity.HasOne(t => t.RegionId)
                .WithMany()
                .HasForeignKey(t => t.RegionGuidId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ===================== EmployeeTerritoryEntity =====================
        modelBuilder.Entity<EmployeeTerritoryEntity>(entity =>
        {
            entity.HasKey(et => new { et.EmployeeGuidId, et.TerritoryGuidId });

            entity.HasOne(et => et.Employee)
                .WithMany(e => e.EmployeeTerritories)
                .HasForeignKey(et => et.EmployeeGuidId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(et => et.Territory)
                .WithMany(t => t.EmployeeTerritories)
                .HasForeignKey(et => et.TerritoryGuidId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureAuthorization(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserEntity>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(u => u.Id);
            entity.HasIndex(u => u.Name).IsUnique();

            entity.HasData(new UserEntity
            {
                Id = DefaultAdminId,
                Name = DefaultAdminName,
                PasswordHash = DefaultAdminPasswordHash,
            });
        });

        modelBuilder.Entity<RoleEntity>(entity =>
        {
            entity.ToTable("Roles");
            entity.HasKey(r => r.Id);
            entity.HasIndex(r => r.Name).IsUnique();

            entity.HasData(DefaultRoles.Names.Select(role => new RoleEntity { Id = role.Key, Name = role.Value }));
        });

        modelBuilder.Entity<UserRoleEntity>(entity =>
        {
            entity.ToTable("UserRoles");
            entity.HasKey(ur => new { ur.UserId, ur.RoleId });

            entity.HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasData(new UserRoleEntity { UserId = DefaultAdminId, RoleId = DefaultRoles.AdminId });
        });

        modelBuilder.Entity<RolePermissionEntity>(entity =>
        {
            entity.ToTable("RolePermissions");
            entity.HasKey(rp => new { rp.RoleId, rp.Permission });

            entity.HasOne(rp => rp.Role)
                .WithMany(r => r.Permissions)
                .HasForeignKey(rp => rp.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasData(DefaultRoles.PermissionsByRole.SelectMany(role =>
                role.Value.Select(permission => new RolePermissionEntity { RoleId = role.Key, Permission = permission })));
        });
    }
}