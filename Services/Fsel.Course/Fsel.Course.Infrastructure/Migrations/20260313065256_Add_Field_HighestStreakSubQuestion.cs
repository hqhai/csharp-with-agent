using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_Field_HighestStreakSubQuestion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_HomeWorkResults_StudentId",
                table: "HomeWorkResults");

            migrationBuilder.AddColumn<int>(
                name: "HighestStreakSubQuestion",
                table: "VideoTimeCodeResults",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HighestStreakSubQuestion",
                table: "VideoResults",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HighestStreakSubQuestion",
                table: "TestSectionResult",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HighestStreakSubQuestion",
                table: "HomeWorkResults",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_HomeWorkResults_StudentId",
                table: "HomeWorkResults",
                column: "StudentId")
                .Annotation("SqlServer:Include", new[] { "CompletionDate", "CorrectCount", "CorrectTotal", "CreatedDate", "CreatedFullName", "CreatedUserId", "HighestStreak", "HighestStreakSubQuestion", "HomeWorkId", "IsDeleted", "LessonModuleId", "LessonResultId", "NewDate", "Percent", "PercentModule", "ProcessDate", "SkillScoresStr", "Status", "SubmissionCount", "TokenFirstTime", "TokenLastTime", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_HomeWorkResults_StudentId",
                table: "HomeWorkResults");

            migrationBuilder.DropColumn(
                name: "HighestStreakSubQuestion",
                table: "VideoTimeCodeResults");

            migrationBuilder.DropColumn(
                name: "HighestStreakSubQuestion",
                table: "VideoResults");

            migrationBuilder.DropColumn(
                name: "HighestStreakSubQuestion",
                table: "TestSectionResult");

            migrationBuilder.DropColumn(
                name: "HighestStreakSubQuestion",
                table: "HomeWorkResults");

            migrationBuilder.CreateIndex(
                name: "IX_HomeWorkResults_StudentId",
                table: "HomeWorkResults",
                column: "StudentId")
                .Annotation("SqlServer:Include", new[] { "CompletionDate", "CorrectCount", "CorrectTotal", "CreatedDate", "CreatedFullName", "CreatedUserId", "HighestStreak", "HomeWorkId", "IsDeleted", "LessonModuleId", "LessonResultId", "NewDate", "Percent", "PercentModule", "ProcessDate", "SkillScoresStr", "Status", "SubmissionCount", "TokenFirstTime", "TokenLastTime", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" });
        }
    }
}
