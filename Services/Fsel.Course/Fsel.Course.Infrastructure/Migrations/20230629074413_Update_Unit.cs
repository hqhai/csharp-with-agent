using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_Unit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "VideoSkillScoresStr",
                table: "VideoResults",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SkillScoresStr",
                table: "LessonResults",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SkillScoresStr",
                table: "HomeWorkResults",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "Score",
                table: "ClassForumScores",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VideoSkillScoresStr",
                table: "VideoResults");

            migrationBuilder.DropColumn(
                name: "SkillScoresStr",
                table: "LessonResults");

            migrationBuilder.DropColumn(
                name: "SkillScoresStr",
                table: "HomeWorkResults");

            migrationBuilder.AlterColumn<long>(
                name: "Score",
                table: "ClassForumScores",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");
        }
    }
}
