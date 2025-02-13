using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_Index_VideoTimeCodeAnswer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_VideoTimeCodeAnswers_VideoResultId_VideoTimeCodeId_ExerciseId_QuestionId_VideoTimeCodeResultId",
                table: "VideoTimeCodeAnswers");

            migrationBuilder.CreateIndex(
                name: "IX_VideoTimeCodeAnswers_VideoResultId_QuestionId_VideoTimeCodeResultId",
                table: "VideoTimeCodeAnswers",
                columns: new[] { "VideoResultId", "QuestionId", "VideoTimeCodeResultId" },
                unique: true,
                filter: "[VideoResultId] IS NOT NULL AND [VideoTimeCodeResultId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_VideoTimeCodeAnswers_VideoResultId_QuestionId_VideoTimeCodeResultId",
                table: "VideoTimeCodeAnswers");

            migrationBuilder.CreateIndex(
                name: "IX_VideoTimeCodeAnswers_VideoResultId_VideoTimeCodeId_ExerciseId_QuestionId_VideoTimeCodeResultId",
                table: "VideoTimeCodeAnswers",
                columns: new[] { "VideoResultId", "VideoTimeCodeId", "ExerciseId", "QuestionId", "VideoTimeCodeResultId" },
                unique: true,
                filter: "[VideoResultId] IS NOT NULL AND [VideoTimeCodeResultId] IS NOT NULL");
        }
    }
}
