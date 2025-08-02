using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibManage.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIsApprovedToReviewMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "Reviews",
                type: "bit",
                nullable: false,
                defaultValue: false,
                comment: "Is the review approved by a manager?");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "Reviews");
        }
    }
}
