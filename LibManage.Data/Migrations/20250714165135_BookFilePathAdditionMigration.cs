using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibManage.Data.Migrations
{
    /// <inheritdoc />
    public partial class BookFilePathAdditionMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BookFilePath",
                table: "Books",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Where the file is located");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BookFilePath",
                table: "Books");
        }
    }
}
