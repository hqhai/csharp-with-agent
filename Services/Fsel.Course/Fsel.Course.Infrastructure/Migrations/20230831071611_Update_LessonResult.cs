using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_LessonResult : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CorrectCount",
                table: "LessonResults",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CorrectTotal",
                table: "LessonResults",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CorrectCount",
                table: "LessonResults");

            migrationBuilder.DropColumn(
                name: "CorrectTotal",
                table: "LessonResults");
        }
    }
}
