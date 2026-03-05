using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations;

/// <inheritdoc />
public partial class AddViewsAndCreatedAtToGame : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTime>(
            name: "CreatedAt",
            table: "Games",
            type: "datetime2",
            nullable: false,
            defaultValueSql: "GETUTCDATE()");

        migrationBuilder.AddColumn<int>(
            name: "Views",
            table: "Games",
            type: "int",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AlterColumn<string>(
            name: "UserName",
            table: "BanEntity",
            type: "nvarchar(450)",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(max)");

        migrationBuilder.CreateIndex(
            name: "IX_BanEntity_UserName",
            table: "BanEntity",
            column: "UserName");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_BanEntity_UserName",
            table: "BanEntity");

        migrationBuilder.DropColumn(
            name: "CreatedAt",
            table: "Games");

        migrationBuilder.DropColumn(
            name: "Views",
            table: "Games");

        migrationBuilder.AlterColumn<string>(
            name: "UserName",
            table: "BanEntity",
            type: "nvarchar(max)",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)");
    }
}
