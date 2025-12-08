using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations;

/// <inheritdoc />
public partial class AddFileTable : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Files",
            columns: table => new
            {
                FileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                GameId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Files", x => x.FileId);
                table.ForeignKey(
                    name: "FK_Files_Games_GameId",
                    column: x => x.GameId,
                    principalTable: "Games",
                    principalColumn: "GameId");
            });

        migrationBuilder.CreateIndex(
            name: "IX_Files_GameId",
            table: "Files",
            column: "GameId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Files");
    }
}
