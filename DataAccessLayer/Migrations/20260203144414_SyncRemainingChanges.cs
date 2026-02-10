using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations;

/// <inheritdoc />
public partial class SyncRemainingChanges : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Files_Games_GameId",
            table: "Files");

        migrationBuilder.DropForeignKey(
            name: "FK_GameGenres_Games_GameId",
            table: "GameGenres");

        migrationBuilder.DropForeignKey(
            name: "FK_GamePlatforms_Games_GameId",
            table: "GamePlatforms");

        migrationBuilder.DropPrimaryKey(
            name: "PK_Games",
            table: "Games");

        migrationBuilder.DropColumn(
            name: "GameDescription",
            table: "Games");

        migrationBuilder.RenameColumn(
            name: "GameName",
            table: "Games",
            newName: "Name");

        migrationBuilder.RenameColumn(
            name: "GameKey",
            table: "Games",
            newName: "Key");

        migrationBuilder.RenameColumn(
            name: "GameId",
            table: "Games",
            newName: "PublisherId");

        migrationBuilder.RenameIndex(
            name: "IX_Games_GameKey",
            table: "Games",
            newName: "IX_Games_Key");

        migrationBuilder.AddColumn<Guid>(
            name: "Id",
            table: "Games",
            type: "uniqueidentifier",
            nullable: false,
            defaultValue: Guid.Empty);

        migrationBuilder.AddColumn<string>(
            name: "Description",
            table: "Games",
            type: "nvarchar(3000)",
            maxLength: 3000,
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "Discount",
            table: "Games",
            type: "int",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<double>(
            name: "Price",
            table: "Games",
            type: "float",
            nullable: false,
            defaultValue: 0.0);

        migrationBuilder.AddColumn<int>(
            name: "UnitInStock",
            table: "Games",
            type: "int",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddPrimaryKey(
            name: "PK_Games",
            table: "Games",
            column: "Id");

        migrationBuilder.CreateTable(
            name: "PublisherEntity",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CompanyName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                HomePage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PublisherEntity", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Games_PublisherId",
            table: "Games",
            column: "PublisherId");

        migrationBuilder.AddForeignKey(
            name: "FK_Files_Games_GameId",
            table: "Files",
            column: "GameId",
            principalTable: "Games",
            principalColumn: "Id");

        migrationBuilder.AddForeignKey(
            name: "FK_GameGenres_Games_GameId",
            table: "GameGenres",
            column: "GameId",
            principalTable: "Games",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_GamePlatforms_Games_GameId",
            table: "GamePlatforms",
            column: "GameId",
            principalTable: "Games",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_Games_PublisherEntity_PublisherId",
            table: "Games",
            column: "PublisherId",
            principalTable: "PublisherEntity",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Files_Games_GameId",
            table: "Files");

        migrationBuilder.DropForeignKey(
            name: "FK_GameGenres_Games_GameId",
            table: "GameGenres");

        migrationBuilder.DropForeignKey(
            name: "FK_GamePlatforms_Games_GameId",
            table: "GamePlatforms");

        migrationBuilder.DropForeignKey(
            name: "FK_Games_PublisherEntity_PublisherId",
            table: "Games");

        migrationBuilder.DropTable(
            name: "PublisherEntity");

        migrationBuilder.DropPrimaryKey(
            name: "PK_Games",
            table: "Games");

        migrationBuilder.DropIndex(
            name: "IX_Games_PublisherId",
            table: "Games");

        migrationBuilder.DropColumn(
            name: "Id",
            table: "Games");

        migrationBuilder.DropColumn(
            name: "Description",
            table: "Games");

        migrationBuilder.DropColumn(
            name: "Discount",
            table: "Games");

        migrationBuilder.DropColumn(
            name: "Price",
            table: "Games");

        migrationBuilder.DropColumn(
            name: "UnitInStock",
            table: "Games");

        migrationBuilder.RenameColumn(
            name: "PublisherId",
            table: "Games",
            newName: "GameId");

        migrationBuilder.RenameColumn(
            name: "Name",
            table: "Games",
            newName: "GameName");

        migrationBuilder.RenameColumn(
            name: "Key",
            table: "Games",
            newName: "GameKey");

        migrationBuilder.RenameIndex(
            name: "IX_Games_Key",
            table: "Games",
            newName: "IX_Games_GameKey");

        migrationBuilder.AddColumn<string>(
            name: "GameDescription",
            table: "Games",
            type: "nvarchar(1000)",
            maxLength: 1000,
            nullable: true);

        migrationBuilder.AddPrimaryKey(
            name: "PK_Games",
            table: "Games",
            column: "GameId");

        migrationBuilder.AddForeignKey(
            name: "FK_Files_Games_GameId",
            table: "Files",
            column: "GameId",
            principalTable: "Games",
            principalColumn: "GameId");

        migrationBuilder.AddForeignKey(
            name: "FK_GameGenres_Games_GameId",
            table: "GameGenres",
            column: "GameId",
            principalTable: "Games",
            principalColumn: "GameId",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_GamePlatforms_Games_GameId",
            table: "GamePlatforms",
            column: "GameId",
            principalTable: "Games",
            principalColumn: "GameId",
            onDelete: ReferentialAction.Cascade);
    }
}
