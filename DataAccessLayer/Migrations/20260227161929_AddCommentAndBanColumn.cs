using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations;

/// <inheritdoc />
public partial class AddCommentAndBanColumn : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "BanEntity",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                UserName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                ExpireAt = table.Column<DateTime>(type: "datetime2", nullable: true),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_BanEntity", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "CommentEntity",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                ParentCommentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                GameId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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

        migrationBuilder.CreateIndex(
            name: "IX_CommentEntity_GameId",
            table: "CommentEntity",
            column: "GameId");

        migrationBuilder.CreateIndex(
            name: "IX_CommentEntity_ParentCommentId",
            table: "CommentEntity",
            column: "ParentCommentId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "BanEntity");

        migrationBuilder.DropTable(
            name: "CommentEntity");
    }
}
