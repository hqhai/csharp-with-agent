using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_HomeWork_HighestStreak : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_HomeWorkResults_StudentId",
                table: "HomeWorkResults");

            migrationBuilder.AddColumn<int>(
                name: "HighestStreak",
                table: "HomeWorkResults",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HighestStreak",
                table: "HomeWorkExtraPracticeResults",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFirstSubmit",
                table: "HomeWorkExtraPracticeAnswers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsFirstSubmit",
                table: "HomeWorkAnswers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_HomeWorkResults_StudentId",
                table: "HomeWorkResults",
                column: "StudentId")
                .Annotation("SqlServer:Include", new[] { "CompletionDate", "CorrectCount", "CorrectTotal", "CreatedDate", "CreatedFullName", "CreatedUserId", "HighestStreak", "HomeWorkId", "IsDeleted", "LessonModuleId", "LessonResultId", "NewDate", "Percent", "PercentModule", "ProcessDate", "SkillScoresStr", "Status", "SubmissionCount", "TokenFirstTime", "TokenLastTime", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_HomeWorkResults_StudentId",
                table: "HomeWorkResults");

            migrationBuilder.DropColumn(
                name: "HighestStreak",
                table: "HomeWorkResults");

            migrationBuilder.DropColumn(
                name: "HighestStreak",
                table: "HomeWorkExtraPracticeResults");

            migrationBuilder.DropColumn(
                name: "IsFirstSubmit",
                table: "HomeWorkExtraPracticeAnswers");

            migrationBuilder.DropColumn(
                name: "IsFirstSubmit",
                table: "HomeWorkAnswers");

            migrationBuilder.CreateIndex(
                name: "IX_HomeWorkResults_StudentId",
                table: "HomeWorkResults",
                column: "StudentId")
                .Annotation("SqlServer:Include", new[] { "CompletionDate", "CorrectCount", "CorrectTotal", "CreatedDate", "CreatedFullName", "CreatedUserId", "HomeWorkId", "IsDeleted", "LessonModuleId", "LessonResultId", "NewDate", "Percent", "PercentModule", "ProcessDate", "SkillScoresStr", "Status", "SubmissionCount", "TokenFirstTime", "TokenLastTime", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" });
        }
    }
}
