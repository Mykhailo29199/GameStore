using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations;

/// <inheritdoc />
public partial class FixPlatformsColumns : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameColumn(
            name: "PlatformType",
            table: "Platforms",
            newName: "Type");

        migrationBuilder.RenameColumn(
            name: "PlatformId",
            table: "Platforms",
            newName: "Id");

        migrationBuilder.RenameIndex(
            name: "IX_Platforms_PlatformType",
            table: "Platforms",
            newName: "IX_Platforms_Type");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameColumn(
            name: "Type",
            table: "Platforms",
            newName: "PlatformType");

        migrationBuilder.RenameColumn(
            name: "Id",
            table: "Platforms",
            newName: "PlatformId");

        migrationBuilder.RenameIndex(
            name: "IX_Platforms_Type",
            table: "Platforms",
            newName: "IX_Platforms_PlatformType");
    }
}
