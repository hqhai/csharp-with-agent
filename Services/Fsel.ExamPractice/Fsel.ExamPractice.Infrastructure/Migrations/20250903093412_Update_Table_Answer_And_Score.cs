using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.ExamPractice.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_Table_Answer_And_Score : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AIType",
                table: "ProsodyScores",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Language",
                table: "ProsodyScores",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModuleAIType",
                table: "ProsodyScores",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "GradingAlFeedback",
                table: "ExamPracticeScores",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PronunciationAssessmentStr",
                table: "ExamPracticeAnswers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AIType",
                table: "ProsodyScores");

            migrationBuilder.DropColumn(
                name: "Language",
                table: "ProsodyScores");

            migrationBuilder.DropColumn(
                name: "ModuleAIType",
                table: "ProsodyScores");

            migrationBuilder.DropColumn(
                name: "GradingAlFeedback",
                table: "ExamPracticeScores");

            migrationBuilder.DropColumn(
                name: "PronunciationAssessmentStr",
                table: "ExamPracticeAnswers");
        }
    }
}
