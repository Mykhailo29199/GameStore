using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations;

/// <inheritdoc />
public partial class RefactorGamesAndFiles : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Files_Games_GameId",
            table: "Files");

        migrationBuilder.DropIndex(
            name: "IX_Games_Key",
            table: "Games");

        migrationBuilder.DropPrimaryKey(
            name: "PK_Files",
            table: "Files");

        migrationBuilder.DropIndex(
            name: "IX_Files_GameId",
            table: "Files");

        migrationBuilder.DropColumn(
            name: "FileId",
            table: "Files");

        migrationBuilder.AlterColumn<Guid>(
            name: "GameId",
            table: "Files",
            type: "uniqueidentifier",
            nullable: false,
            defaultValue: Guid.Empty,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AddPrimaryKey(
            name: "PK_Files",
            table: "Files",
            column: "GameId");

        migrationBuilder.AddForeignKey(
            name: "FK_Files_Games_GameId",
            table: "Files",
            column: "GameId",
            principalTable: "Games",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Files_Games_GameId",
            table: "Files");

        migrationBuilder.DropPrimaryKey(
            name: "PK_Files",
            table: "Files");

        migrationBuilder.AlterColumn<Guid>(
            name: "GameId",
            table: "Files",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier");

        migrationBuilder.AddColumn<Guid>(
            name: "FileId",
            table: "Files",
            type: "uniqueidentifier",
            nullable: false,
            defaultValue: Guid.Empty);

        migrationBuilder.AddPrimaryKey(
            name: "PK_Files",
            table: "Files",
            column: "FileId");

        migrationBuilder.CreateIndex(
            name: "IX_Games_Key",
            table: "Games",
            column: "Key",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Files_GameId",
            table: "Files",
            column: "GameId");

        migrationBuilder.AddForeignKey(
            name: "FK_Files_Games_GameId",
            table: "Files",
            column: "GameId",
            principalTable: "Games",
            principalColumn: "Id");
    }
}
