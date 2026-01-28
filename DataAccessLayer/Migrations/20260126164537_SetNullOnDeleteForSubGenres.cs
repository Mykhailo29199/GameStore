using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations;

/// <inheritdoc />
public partial class SetNullOnDeleteForSubGenres : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Genres_Genres_ParentGenreId",
            table: "Genres");

        migrationBuilder.AddForeignKey(
            name: "FK_Genres_Genres_ParentGenreId",
            table: "Genres",
            column: "ParentGenreId",
            principalTable: "Genres",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Genres_Genres_ParentGenreId",
            table: "Genres");

        migrationBuilder.AddForeignKey(
            name: "FK_Genres_Genres_ParentGenreId",
            table: "Genres",
            column: "ParentGenreId",
            principalTable: "Genres",
            principalColumn: "Id");
    }
}
