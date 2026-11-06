using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations;

/// <inheritdoc />
public partial class AddLocalization : Migration
{
    // Returns the games in the requested language; a game without a translation, or a field without one, stays in English.
    private const string CreateGetLocalizedGamesFunction = """
        CREATE FUNCTION dbo.GetLocalizedGames (@LanguageCode nvarchar(10))
        RETURNS TABLE
        AS
        RETURN
            SELECT
                p.Id,
                p.[Key],
                COALESCE(t.Name, p.ProductName) AS Name,
                COALESCE(t.Description, p.Description) AS Description,
                p.UnitPrice,
                p.Discount,
                p.UnitsInStock,
                CASE WHEN t.ProductId IS NULL THEN N'en' ELSE t.LanguageCode END AS LanguageCode
            FROM dbo.Products AS p
            LEFT JOIN dbo.ProductTranslations AS t
                ON t.ProductId = p.Id AND t.LanguageCode = @LanguageCode
        """;

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "CategoryTranslations",
            columns: table => new
            {
                LanguageCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_CategoryTranslations", x => new { x.CategoryId, x.LanguageCode });
                table.ForeignKey(
                    name: "FK_CategoryTranslations_Categories_CategoryId",
                    column: x => x.CategoryId,
                    principalTable: "Categories",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "PlatformTranslations",
            columns: table => new
            {
                LanguageCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                PlatformId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Type = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PlatformTranslations", x => new { x.PlatformId, x.LanguageCode });
                table.ForeignKey(
                    name: "FK_PlatformTranslations_Platforms_PlatformId",
                    column: x => x.PlatformId,
                    principalTable: "Platforms",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ProductTranslations",
            columns: table => new
            {
                LanguageCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                Description = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: true),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ProductTranslations", x => new { x.ProductId, x.LanguageCode });
                table.ForeignKey(
                    name: "FK_ProductTranslations_Products_ProductId",
                    column: x => x.ProductId,
                    principalTable: "Products",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "SupplierTranslations",
            columns: table => new
            {
                LanguageCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                SupplierId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Description = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: true),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SupplierTranslations", x => new { x.SupplierId, x.LanguageCode });
                table.ForeignKey(
                    name: "FK_SupplierTranslations_Suppliers_SupplierId",
                    column: x => x.SupplierId,
                    principalTable: "Suppliers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.Sql(CreateGetLocalizedGamesFunction);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DROP FUNCTION IF EXISTS dbo.GetLocalizedGames");

        migrationBuilder.DropTable(
            name: "CategoryTranslations");

        migrationBuilder.DropTable(
            name: "PlatformTranslations");

        migrationBuilder.DropTable(
            name: "ProductTranslations");

        migrationBuilder.DropTable(
            name: "SupplierTranslations");
    }
}
