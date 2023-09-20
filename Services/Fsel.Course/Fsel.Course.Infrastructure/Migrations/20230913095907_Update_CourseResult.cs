using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_CourseResult : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Result",
                table: "CourseResults",
                newName: "CorrectTotal");

            migrationBuilder.AddColumn<int>(
                name: "CorrectCount",
                table: "CourseResults",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "Percent",
                table: "CourseResults",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "SkillScoresStr",
                table: "CourseResults",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CorrectCount",
                table: "CourseResults");

            migrationBuilder.DropColumn(
                name: "Percent",
                table: "CourseResults");

            migrationBuilder.DropColumn(
                name: "SkillScoresStr",
                table: "CourseResults");

            migrationBuilder.RenameColumn(
                name: "CorrectTotal",
                table: "CourseResults",
                newName: "Result");
        }
    }
}
