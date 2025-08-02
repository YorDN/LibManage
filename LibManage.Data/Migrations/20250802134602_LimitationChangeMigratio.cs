using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibManage.Data.Migrations
{
    /// <inheritdoc />
    public partial class LimitationChangeMigratio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Publishers",
                type: "nvarchar(max)",
                nullable: true,
                comment: "The description of the publisher",
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true,
                oldComment: "The description of the publisher");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Books",
                type: "nvarchar(max)",
                nullable: true,
                comment: "A short description of the book",
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true,
                oldComment: "A short description of the book");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Publishers",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                comment: "The description of the publisher",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "The description of the publisher");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Books",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                comment: "A short description of the book",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "A short description of the book");
        }
    }
}
