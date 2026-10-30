using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations;

/// <inheritdoc />
public partial class NorthwindIntegration : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // BanEntity.UserName is already nvarchar(450) with IX_BanEntity_UserName: both were created by
        // AddViewsAndCreatedAtToGame, but the epic 7 model snapshot lost them, so they are not repeated here.
        migrationBuilder.RenameColumn(
            name: "Date",
            table: "Orders",
            newName: "OrderDate");

        migrationBuilder.AddColumn<string>(
            name: "Field14",
            table: "Orders",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<double>(
            name: "Freight",
            table: "Orders",
            type: "float",
            nullable: true);

        migrationBuilder.AddColumn<DateTime>(
            name: "ShippedDate",
            table: "Orders",
            type: "datetime2",
            nullable: true);

        migrationBuilder.AddColumn<DateTime>(
            name: "RequiredDate",
            table: "Orders",
            type: "datetime2",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "ShipAddress",
            table: "Orders",
            type: "nvarchar(300)",
            maxLength: 300,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "ShipCity",
            table: "Orders",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "ShipCountry",
            table: "Orders",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "ShipName",
            table: "Orders",
            type: "nvarchar(200)",
            maxLength: 200,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "ShipPostalCode",
            table: "Orders",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "ShipRegion",
            table: "Orders",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "ShipVia",
            table: "Orders",
            type: "int",
            nullable: true);

        migrationBuilder.AddColumn<Guid>(
            name: "ShipperId",
            table: "Orders",
            type: "uniqueidentifier",
            nullable: true);

        migrationBuilder.CreateTable(
            name: "Categories",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ParentCategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                CategoryID = table.Column<int>(type: "int", nullable: true),
                Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                Picture = table.Column<string>(type: "nvarchar(max)", nullable: true),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Categories", x => x.Id);
                table.ForeignKey(
                    name: "FK_Categories_Categories_ParentCategoryId",
                    column: x => x.ParentCategoryId,
                    principalTable: "Categories",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "CustomerEntity",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CustomerID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                CompanyName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                ContactName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                ContactTitle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                City = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Region = table.Column<string>(type: "nvarchar(max)", nullable: true),
                PostalCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Country = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Phone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Fax = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Field11 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_CustomerEntity", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Employees",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ReportsToId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                EmployeeID = table.Column<int>(type: "int", nullable: true),
                LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                TitleOfCourtesy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                BirthDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                HireDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                City = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Region = table.Column<string>(type: "nvarchar(max)", nullable: true),
                PostalCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Country = table.Column<string>(type: "nvarchar(max)", nullable: true),
                HomePhone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Extension = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                PhotoPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Field18 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Field19 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Field20 = table.Column<string>(type: "nvarchar(max)", nullable: true),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Employees", x => x.Id);
                table.ForeignKey(
                    name: "FK_Employees_Employees_ReportsToId",
                    column: x => x.ReportsToId,
                    principalTable: "Employees",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "Regions",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                RegionID = table.Column<int>(type: "int", nullable: true),
                RegionDescription = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Regions", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Shippers",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ShipperID = table.Column<int>(type: "int", nullable: true),
                CompanyName = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                Phone = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: true),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Shippers", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Suppliers",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                SupplierID = table.Column<int>(type: "int", nullable: true),
                CompanyName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                ContactName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                ContactTitle = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                Address = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                Region = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                PostalCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                Fax = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                HomePage = table.Column<string>(type: "nvarchar(333)", maxLength: 333, nullable: true),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Suppliers", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Territories",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                RegionGuidId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                TerritoryID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                TerritoryDescription = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                RegionID = table.Column<int>(type: "int", nullable: true),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Territories", x => x.Id);
                table.ForeignKey(
                    name: "FK_Territories_Regions_RegionGuidId",
                    column: x => x.RegionGuidId,
                    principalTable: "Regions",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Products",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Description = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: true),
                Discount = table.Column<double>(type: "float", nullable: false, defaultValue: 0.0),
                Views = table.Column<int>(type: "int", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                SupplierID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                ProductID = table.Column<int>(type: "int", nullable: true),
                ProductName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                Key = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                UnitPrice = table.Column<double>(type: "float", nullable: false),
                UnitsInStock = table.Column<int>(type: "int", nullable: false),
                UnitsOnOrder = table.Column<int>(type: "int", nullable: false),
                ReorderLevel = table.Column<int>(type: "int", nullable: false),
                QuantityPerUnit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                Discontinued = table.Column<int>(type: "int", nullable: true),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Products", x => x.Id);
                table.ForeignKey(
                    name: "FK_Products_Suppliers_SupplierID",
                    column: x => x.SupplierID,
                    principalTable: "Suppliers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "EmployeeTerritories",
            columns: table => new
            {
                EmployeeGuidId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                TerritoryGuidId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                EmployeeID = table.Column<int>(type: "int", nullable: true),
                TerritoryID = table.Column<string>(type: "nvarchar(max)", nullable: true),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_EmployeeTerritories", x => new { x.EmployeeGuidId, x.TerritoryGuidId });
                table.ForeignKey(
                    name: "FK_EmployeeTerritories_Employees_EmployeeGuidId",
                    column: x => x.EmployeeGuidId,
                    principalTable: "Employees",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_EmployeeTerritories_Territories_TerritoryGuidId",
                    column: x => x.TerritoryGuidId,
                    principalTable: "Territories",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Comments",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                ParentCommentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Comments", x => x.Id);
                table.ForeignKey(
                    name: "FK_Comments_Comments_ParentCommentId",
                    column: x => x.ParentCommentId,
                    principalTable: "Comments",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_Comments_Products_ProductId",
                    column: x => x.ProductId,
                    principalTable: "Products",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "OrderDetails",
            columns: table => new
            {
                OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Price = table.Column<double>(type: "float", nullable: false),
                Quantity = table.Column<int>(type: "int", nullable: false),
                Discount = table.Column<double>(type: "float", nullable: true, defaultValue: 0.0),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_OrderDetails", x => new { x.OrderId, x.ProductId });
                table.ForeignKey(
                    name: "FK_OrderDetails_Orders_OrderId",
                    column: x => x.OrderId,
                    principalTable: "Orders",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_OrderDetails_Products_ProductId",
                    column: x => x.ProductId,
                    principalTable: "Products",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ProductCategories",
            columns: table => new
            {
                ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ProductCategories", x => new { x.ProductId, x.CategoryId });
                table.ForeignKey(
                    name: "FK_ProductCategories_Categories_CategoryId",
                    column: x => x.CategoryId,
                    principalTable: "Categories",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_ProductCategories_Products_ProductId",
                    column: x => x.ProductId,
                    principalTable: "Products",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ProductFiles",
            columns: table => new
            {
                ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ProductFiles", x => x.ProductId);
                table.ForeignKey(
                    name: "FK_ProductFiles_Products_ProductId",
                    column: x => x.ProductId,
                    principalTable: "Products",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ProductPlatforms",
            columns: table => new
            {
                ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                PlatformId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ProductPlatforms", x => new { x.ProductId, x.PlatformId });
                table.ForeignKey(
                    name: "FK_ProductPlatforms_Platforms_PlatformId",
                    column: x => x.PlatformId,
                    principalTable: "Platforms",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_ProductPlatforms_Products_ProductId",
                    column: x => x.ProductId,
                    principalTable: "Products",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        // Move epic 7 data into the renamed Northwind-compatible tables before the old tables are dropped.
        migrationBuilder.Sql(@"
INSERT INTO Suppliers (Id, CompanyName, Description, HomePage, IsDeleted)
SELECT Id, CompanyName, Description, HomePage, 0 FROM PublisherEntity;

INSERT INTO Categories (Id, Name, ParentCategoryId, IsDeleted)
SELECT Id, Name, ParentGenreId, 0 FROM Genres;

INSERT INTO Products (Id, ProductName, [Key], Description, UnitPrice, UnitsInStock, UnitsOnOrder, ReorderLevel, Discount, Views, CreatedAt, SupplierID, IsDeleted)
SELECT Id, Name, [Key], Description, Price, UnitInStock, 0, 0, Discount, Views, CreatedAt, PublisherId, 0 FROM Games;

INSERT INTO ProductCategories (ProductId, CategoryId)
SELECT GameId, GenreId FROM GameGenres;

INSERT INTO ProductPlatforms (ProductId, PlatformId)
SELECT GameId, PlatformId FROM GamePlatforms;

INSERT INTO ProductFiles (ProductId, FilePath)
SELECT GameId, FilePath FROM Files;

INSERT INTO Comments (Id, Name, Body, ParentCommentId, IsDeleted, ProductId)
SELECT Id, Name, Body, ParentCommentId, IsDeleted, GameId FROM CommentEntity;

INSERT INTO OrderDetails (OrderId, ProductId, Price, Quantity, Discount)
SELECT OrderId, ProductId, Price, Quantity, Discount FROM OrderGames;
");

        migrationBuilder.DropTable(
            name: "CommentEntity");

        migrationBuilder.DropTable(
            name: "Files");

        migrationBuilder.DropTable(
            name: "GameGenres");

        migrationBuilder.DropTable(
            name: "GamePlatforms");

        migrationBuilder.DropTable(
            name: "OrderGames");

        migrationBuilder.DropTable(
            name: "Genres");

        migrationBuilder.DropTable(
            name: "Games");

        migrationBuilder.DropTable(
            name: "PublisherEntity");

        migrationBuilder.CreateIndex(
            name: "IX_Orders_ShipperId",
            table: "Orders",
            column: "ShipperId");

        migrationBuilder.CreateIndex(
            name: "IX_Categories_ParentCategoryId",
            table: "Categories",
            column: "ParentCategoryId");

        migrationBuilder.CreateIndex(
            name: "IX_Comments_ParentCommentId",
            table: "Comments",
            column: "ParentCommentId");

        migrationBuilder.CreateIndex(
            name: "IX_Comments_ProductId",
            table: "Comments",
            column: "ProductId");

        migrationBuilder.CreateIndex(
            name: "IX_Employees_ReportsToId",
            table: "Employees",
            column: "ReportsToId");

        migrationBuilder.CreateIndex(
            name: "IX_EmployeeTerritories_TerritoryGuidId",
            table: "EmployeeTerritories",
            column: "TerritoryGuidId");

        migrationBuilder.CreateIndex(
            name: "IX_OrderDetails_ProductId",
            table: "OrderDetails",
            column: "ProductId");

        migrationBuilder.CreateIndex(
            name: "IX_ProductCategories_CategoryId",
            table: "ProductCategories",
            column: "CategoryId");

        migrationBuilder.CreateIndex(
            name: "IX_ProductPlatforms_PlatformId",
            table: "ProductPlatforms",
            column: "PlatformId");

        migrationBuilder.CreateIndex(
            name: "IX_Products_Key",
            table: "Products",
            column: "Key",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Products_SupplierID",
            table: "Products",
            column: "SupplierID");

        migrationBuilder.CreateIndex(
            name: "IX_Territories_RegionGuidId",
            table: "Territories",
            column: "RegionGuidId");

        migrationBuilder.AddForeignKey(
            name: "FK_Orders_Shippers_ShipperId",
            table: "Orders",
            column: "ShipperId",
            principalTable: "Shippers",
            principalColumn: "Id");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Orders_Shippers_ShipperId",
            table: "Orders");

        migrationBuilder.DropIndex(
            name: "IX_Orders_ShipperId",
            table: "Orders");

        migrationBuilder.DropColumn(
            name: "Field14",
            table: "Orders");

        migrationBuilder.DropColumn(
            name: "Freight",
            table: "Orders");

        migrationBuilder.DropColumn(
            name: "ShippedDate",
            table: "Orders");

        migrationBuilder.DropColumn(
            name: "RequiredDate",
            table: "Orders");

        migrationBuilder.DropColumn(
            name: "ShipAddress",
            table: "Orders");

        migrationBuilder.DropColumn(
            name: "ShipCity",
            table: "Orders");

        migrationBuilder.DropColumn(
            name: "ShipCountry",
            table: "Orders");

        migrationBuilder.DropColumn(
            name: "ShipName",
            table: "Orders");

        migrationBuilder.DropColumn(
            name: "ShipPostalCode",
            table: "Orders");

        migrationBuilder.DropColumn(
            name: "ShipRegion",
            table: "Orders");

        migrationBuilder.DropColumn(
            name: "ShipVia",
            table: "Orders");

        migrationBuilder.DropColumn(
            name: "ShipperId",
            table: "Orders");

        migrationBuilder.RenameColumn(
            name: "OrderDate",
            table: "Orders",
            newName: "Date");

        migrationBuilder.CreateTable(
            name: "Genres",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ParentGenreId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Genres", x => x.Id);
                table.ForeignKey(
                    name: "FK_Genres_Genres_ParentGenreId",
                    column: x => x.ParentGenreId,
                    principalTable: "Genres",
                    principalColumn: "Id");
            });

        migrationBuilder.CreateTable(
            name: "PublisherEntity",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CompanyName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                HomePage = table.Column<string>(type: "nvarchar(max)", nullable: true),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PublisherEntity", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Games",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                PublisherId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                Description = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: true),
                Discount = table.Column<int>(type: "int", nullable: false),
                Key = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                Price = table.Column<double>(type: "float", nullable: false),
                UnitInStock = table.Column<int>(type: "int", nullable: false),
                Views = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Games", x => x.Id);
                table.ForeignKey(
                    name: "FK_Games_PublisherEntity_PublisherId",
                    column: x => x.PublisherId,
                    principalTable: "PublisherEntity",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "CommentEntity",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                GameId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                ParentCommentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_CommentEntity", x => x.Id);
                table.ForeignKey(
                    name: "FK_CommentEntity_CommentEntity_ParentCommentId",
                    column: x => x.ParentCommentId,
                    principalTable: "CommentEntity",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_CommentEntity_Games_GameId",
                    column: x => x.GameId,
                    principalTable: "Games",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Files",
            columns: table => new
            {
                GameId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Files", x => x.GameId);
                table.ForeignKey(
                    name: "FK_Files_Games_GameId",
                    column: x => x.GameId,
                    principalTable: "Games",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "GameGenres",
            columns: table => new
            {
                GameId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                GenreId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_GameGenres", x => new { x.GameId, x.GenreId });
                table.ForeignKey(
                    name: "FK_GameGenres_Games_GameId",
                    column: x => x.GameId,
                    principalTable: "Games",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_GameGenres_Genres_GenreId",
                    column: x => x.GenreId,
                    principalTable: "Genres",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "GamePlatforms",
            columns: table => new
            {
                GameId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                PlatformId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_GamePlatforms", x => new { x.GameId, x.PlatformId });
                table.ForeignKey(
                    name: "FK_GamePlatforms_Games_GameId",
                    column: x => x.GameId,
                    principalTable: "Games",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_GamePlatforms_Platforms_PlatformId",
                    column: x => x.PlatformId,
                    principalTable: "Platforms",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "OrderGames",
            columns: table => new
            {
                OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Discount = table.Column<int>(type: "int", nullable: true),
                Price = table.Column<double>(type: "float", nullable: false),
                Quantity = table.Column<int>(type: "int", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_OrderGames", x => new { x.OrderId, x.ProductId });
                table.ForeignKey(
                    name: "FK_OrderGames_Games_ProductId",
                    column: x => x.ProductId,
                    principalTable: "Games",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_OrderGames_Orders_OrderId",
                    column: x => x.OrderId,
                    principalTable: "Orders",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_CommentEntity_GameId",
            table: "CommentEntity",
            column: "GameId");

        migrationBuilder.CreateIndex(
            name: "IX_CommentEntity_ParentCommentId",
            table: "CommentEntity",
            column: "ParentCommentId");

        migrationBuilder.CreateIndex(
            name: "IX_GameGenres_GenreId",
            table: "GameGenres",
            column: "GenreId");

        migrationBuilder.CreateIndex(
            name: "IX_GamePlatforms_PlatformId",
            table: "GamePlatforms",
            column: "PlatformId");

        migrationBuilder.CreateIndex(
            name: "IX_Games_PublisherId",
            table: "Games",
            column: "PublisherId");

        migrationBuilder.CreateIndex(
            name: "IX_Genres_ParentGenreId",
            table: "Genres",
            column: "ParentGenreId");

        migrationBuilder.CreateIndex(
            name: "IX_OrderGames_ProductId",
            table: "OrderGames",
            column: "ProductId");

        // Move data back into the epic 7 tables. Rows soft-deleted by the Game Store are not restored.
        migrationBuilder.Sql(@"
INSERT INTO PublisherEntity (Id, CompanyName, Description, HomePage)
SELECT Id, CompanyName, Description, HomePage FROM Suppliers WHERE IsDeleted = 0;

INSERT INTO Genres (Id, Name, ParentGenreId)
SELECT c.Id, c.Name, CASE WHEN p.Id IS NOT NULL AND p.IsDeleted = 0 THEN c.ParentCategoryId END
FROM Categories c LEFT JOIN Categories p ON p.Id = c.ParentCategoryId
WHERE c.IsDeleted = 0;

INSERT INTO Games (Id, Name, [Key], Description, Price, UnitInStock, Discount, Views, CreatedAt, PublisherId)
SELECT Id, ProductName, [Key], Description, UnitPrice, UnitsInStock, CAST(Discount AS int), Views, CreatedAt, SupplierID
FROM Products WHERE IsDeleted = 0 AND SupplierID IN (SELECT Id FROM PublisherEntity);

INSERT INTO GameGenres (GameId, GenreId)
SELECT ProductId, CategoryId FROM ProductCategories
WHERE ProductId IN (SELECT Id FROM Games) AND CategoryId IN (SELECT Id FROM Genres);

INSERT INTO GamePlatforms (GameId, PlatformId)
SELECT ProductId, PlatformId FROM ProductPlatforms WHERE ProductId IN (SELECT Id FROM Games);

INSERT INTO Files (GameId, FilePath)
SELECT ProductId, FilePath FROM ProductFiles WHERE ProductId IN (SELECT Id FROM Games);

INSERT INTO CommentEntity (Id, Name, Body, ParentCommentId, IsDeleted, GameId)
SELECT Id, Name, Body, ParentCommentId, IsDeleted, ProductId FROM Comments WHERE ProductId IN (SELECT Id FROM Games);

INSERT INTO OrderGames (OrderId, ProductId, Price, Quantity, Discount)
SELECT OrderId, ProductId, Price, Quantity, CAST(Discount AS int) FROM OrderDetails WHERE ProductId IN (SELECT Id FROM Games);
");

        migrationBuilder.DropTable(
            name: "Comments");

        migrationBuilder.DropTable(
            name: "CustomerEntity");

        migrationBuilder.DropTable(
            name: "EmployeeTerritories");

        migrationBuilder.DropTable(
            name: "OrderDetails");

        migrationBuilder.DropTable(
            name: "ProductCategories");

        migrationBuilder.DropTable(
            name: "ProductFiles");

        migrationBuilder.DropTable(
            name: "ProductPlatforms");

        migrationBuilder.DropTable(
            name: "Shippers");

        migrationBuilder.DropTable(
            name: "Employees");

        migrationBuilder.DropTable(
            name: "Territories");

        migrationBuilder.DropTable(
            name: "Categories");

        migrationBuilder.DropTable(
            name: "Products");

        migrationBuilder.DropTable(
            name: "Regions");

        migrationBuilder.DropTable(
            name: "Suppliers");
    }
}