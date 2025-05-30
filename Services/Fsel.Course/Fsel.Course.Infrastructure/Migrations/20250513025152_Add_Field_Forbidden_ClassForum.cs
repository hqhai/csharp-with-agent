using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_Field_Forbidden_ClassForum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GradingAiForbidden",
                table: "ClassForumDetailResults",
                type: "nvarchar(max)",
                maxLength: 10000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsForbiddenImage",
                table: "ClassForumDetailResults",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsForbiddenWork",
                table: "ClassForumDetailResults",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GradingAiForbidden",
                table: "ClassForumDetailResults");

            migrationBuilder.DropColumn(
                name: "IsForbiddenImage",
                table: "ClassForumDetailResults");

            migrationBuilder.DropColumn(
                name: "IsForbiddenWork",
                table: "ClassForumDetailResults");
        }
    }
}
