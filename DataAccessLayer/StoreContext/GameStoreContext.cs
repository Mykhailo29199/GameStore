using DataAccessLayer.Entities.GameStoreSQLEntities;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.StoreContext;

public class GameStoreContext(DbContextOptions<GameStoreContext> options) : DbContext(options)
{
    // Money is stored as decimal(18, 2), so prices and totals have no floating-point rounding errors.
    private const int MoneyPrecision = 18;
    private const int MoneyScale = 2;

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

    // Entities from Northwind Legacy
    public DbSet<EmployeeEntity> Employees { get; set; }

    public DbSet<TerritoryEntity> Territories { get; set; }

    public DbSet<RegionEntity> Regions { get; set; }

    public DbSet<EmployeeTerritoryEntity> EmployeeTerritories { get; set; }

    public DbSet<ShipperEntity> Shippers { get; set; }

    // 1. ПРАВИЛЬНЕ МІСЦЕ ДЛЯ ПРИГЛУШЕННЯ ЛОГІВ
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
            // 1. Первинний ключ
            entity.HasKey(p => p.Id);

            // 2. Налаштування властивостей базового класу (ProductBaseEntity)
            entity.Property(p => p.ProductName)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(p => p.Key)
                .IsRequired()
                .HasMaxLength(250);

            entity.Property(p => p.UnitPrice)
                .HasPrecision(MoneyPrecision, MoneyScale)
                .IsRequired();

            // 3. Налаштування властивостей ProductEntity
            entity.Property(p => p.Description)
                .HasMaxLength(3000);

            // Discount тепер double (0.2), тому в SQL це float
            entity.Property(p => p.Discount)
                .HasColumnType("float")
                .HasDefaultValue(0.0);

            entity.Property(p => p.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // 5. Зв'язок Many-to-Many з Жанрами (Явний мапінг)
            // Використовуємо .WithOne(), щоб уникнути конфлікту імен "GameGenres"
            entity.HasMany(p => p.ProductCategories)
                .WithOne(gg => gg.Product)
                .HasForeignKey(gg => gg.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // 6. Зв'язок Many-to-Many з Платформами (Явний мапінг)
            entity.HasMany(p => p.ProductPlatforms)
                .WithOne(gp => gp.Product)
                .HasForeignKey(gp => gp.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // 7. Зв'язок 1:1 (One-to-One) з ProductFile (файлом гри)
            entity.HasOne(p => p.ProductFile)
                .WithOne(pf => pf.Product)
                .HasForeignKey<ProductFileEntity>(pf => pf.ProductId);

            // 8. Зв'язок 1:N (One-to-Many) з Коментарями
            entity.HasMany(p => p.Comments)
                .WithOne(c => c.Product)
                .HasForeignKey(c => c.ProductId);

            // 9. Індекси
            entity.HasIndex(p => p.Key)
                .IsUnique();

            entity.HasQueryFilter(p => !p.IsDeleted);
        });

        // ===================== GameFileEntity =====================
        modelBuilder.Entity<ProductFileEntity>(entity =>
        {
            entity.HasKey(f => f.ProductId);
        });

        // ===================== ProductCategoryEntity =====================
        modelBuilder.Entity<ProductCategoryEntity>(entity =>
        {
            entity.HasKey(gg => new { gg.ProductId, gg.CategoryId });

            entity.HasOne(gg => gg.Category)
        .WithMany(c => c.ProductCategories) // Тепер EF чітко бачить цю колекцію
        .HasForeignKey(gg => gg.CategoryId)
        .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(gg => gg.Product)
        .WithMany(p => p.ProductCategories)
        .HasForeignKey(gg => gg.ProductId)
        .OnDelete(DeleteBehavior.Cascade);
        });

        // ===================== GamePlatformEntity =====================
        modelBuilder.Entity<ProductPlatformEntity>(entity =>
        {
            entity.HasKey(gp => new { gp.ProductId, gp.PlatformId });

            entity.HasOne(gp => gp.Product)
                .WithMany(g => g.ProductPlatforms) // Вказуємо колекцію в ProductEntity
                .HasForeignKey(gp => gp.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(gp => gp.Platform)
                .WithMany(p => p.ProductPlatforms) // Вказуємо колекцію в PlatformEntity
                .HasForeignKey(gp => gp.PlatformId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ===================== CategoryEntity =====================
        modelBuilder.Entity<CategoryEntity>(entity =>
        {
            entity.ToTable("Categories"); // ГАРАНТІЯ, що він не шукатиме [Category]
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
            entity.ToTable("OrderDetails"); // Вказуємо назву таблиці з вашої бази
            entity.HasKey(og => new { og.OrderId, og.ProductId });

            entity.Property(og => og.UnitPrice)
                .HasColumnName("Price") // Вказуємо назву колонки "Price" як у SQL
                .HasPrecision(MoneyPrecision, MoneyScale)
                .IsRequired();

            entity.Property(og => og.Discount)
                .HasColumnName("Discount")
                .IsRequired(false)
                .HasDefaultValue(0.0); // Якщо в базі NULL, EF підставить 0.0

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
            // Композитний ключ
            entity.HasKey(et => new { et.EmployeeGuidId, et.TerritoryGuidId });

            // Налаштування зв'язків (Many-to-Many)
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
}