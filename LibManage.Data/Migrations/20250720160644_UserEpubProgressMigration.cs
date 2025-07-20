using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibManage.Data.Migrations
{
    /// <inheritdoc />
    public partial class UserEpubProgressMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserEpubProgresses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BookId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LastChapterIndex = table.Column<int>(type: "int", nullable: false, comment: "User's chapter progress"),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false, comment: "When was the progress last updated")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserEpubProgresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserEpubProgresses_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserEpubProgresses_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserEpubProgresses_BookId",
                table: "UserEpubProgresses",
                column: "BookId");

            migrationBuilder.CreateIndex(
                name: "IX_UserEpubProgresses_UserId",
                table: "UserEpubProgresses",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserEpubProgresses");
        }
    }
}
