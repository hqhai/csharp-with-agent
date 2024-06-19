using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRetryClassforumResultTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "ClassForums");

            migrationBuilder.AddColumn<string>(
                name: "RetryContent",
                table: "ClassForumResults",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RetryGradingAlFeedBack",
                table: "ClassForumResults",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RetryWordContent",
                table: "ClassForumResults",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RetryContent",
                table: "ClassForumResults");

            migrationBuilder.DropColumn(
                name: "RetryGradingAlFeedBack",
                table: "ClassForumResults");

            migrationBuilder.DropColumn(
                name: "RetryWordContent",
                table: "ClassForumResults");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "ClassForums",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
