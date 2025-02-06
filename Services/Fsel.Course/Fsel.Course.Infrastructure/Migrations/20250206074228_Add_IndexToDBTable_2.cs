using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_IndexToDBTable_2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_VideoTimeCodeAnswers_QuestionId",
                table: "VideoTimeCodeAnswers");

            migrationBuilder.DropIndex(
                name: "IX_UnitResults_CreatedUserId",
                table: "UnitResults");

            migrationBuilder.CreateIndex(
                name: "IX_VideoTimeCodeResults_Status_StudentId",
                table: "VideoTimeCodeResults",
                columns: new[] { "Status", "StudentId" })
                .Annotation("SqlServer:Include", new[] { "VideoResultId", "VideoTimeCodeId", "CorrectCount", "CorrectTotal", "WorkingTime" });

            migrationBuilder.CreateIndex(
                name: "IX_VideoTimeCodeAnswers_QuestionId_VideoResultId",
                table: "VideoTimeCodeAnswers",
                columns: new[] { "QuestionId", "VideoResultId" })
                .Annotation("SqlServer:Include", new[] { "CorrectCount" });

            migrationBuilder.CreateIndex(
                name: "IX_UnitResults_CreatedUserId",
                table: "UnitResults",
                column: "CreatedUserId")
                .Annotation("SqlServer:Include", new[] { "CompletionDate", "CorrectCount", "CorrectTotal", "CourseId", "CreatedDate", "CreatedFullName", "DeletedDate", "DeletedFullName", "DeletedUserId", "IsDeleted", "Percent", "ProcessDate", "SkillScoresStr", "Status", "StudentId", "UnitId", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" });

            migrationBuilder.CreateIndex(
                name: "IX_TimeCodeExercises_IsDeleted",
                table: "TimeCodeExercises",
                column: "IsDeleted")
                .Annotation("SqlServer:Include", new[] { "ExerciseId", "VideoTimeCodeId" });

            migrationBuilder.CreateIndex(
                name: "IX_PlacementTestResults_CreatedUserId",
                table: "PlacementTestResults",
                column: "CreatedUserId")
                .Annotation("SqlServer:Include", new[] { "CorrectCount", "CorrectTotal", "CountQuestion", "CreatedDate", "CreatedFullName", "DeletedDate", "DeletedFullName", "DeletedUserId", "IsDeleted", "Level", "Percent", "PlacementTestGroupResultId", "PlacementTestId", "SkillScoresStr", "Status", "StudentId", "TotalQuestion", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" });

            migrationBuilder.CreateIndex(
                name: "IX_LessonResults_CreatedUserId",
                table: "LessonResults",
                column: "CreatedUserId")
                .Annotation("SqlServer:Include", new[] { "CorrectCount", "CorrectTotal", "CourseId", "CreatedDate", "CreatedFullName", "DeletedDate", "DeletedFullName", "DeletedUserId", "IsDeleted", "LessonId", "Percent", "SkillScoresStr", "Status", "StudentId", "SummaryNote", "UnitId", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" });

            migrationBuilder.CreateIndex(
                name: "IX_CourseResults_CreatedUserId",
                table: "CourseResults",
                column: "CreatedUserId")
                .Annotation("SqlServer:Include", new[] { "Status", "CourseId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_VideoTimeCodeResults_Status_StudentId",
                table: "VideoTimeCodeResults");

            migrationBuilder.DropIndex(
                name: "IX_VideoTimeCodeAnswers_QuestionId_VideoResultId",
                table: "VideoTimeCodeAnswers");

            migrationBuilder.DropIndex(
                name: "IX_UnitResults_CreatedUserId",
                table: "UnitResults");

            migrationBuilder.DropIndex(
                name: "IX_TimeCodeExercises_IsDeleted",
                table: "TimeCodeExercises");

            migrationBuilder.DropIndex(
                name: "IX_PlacementTestResults_CreatedUserId",
                table: "PlacementTestResults");

            migrationBuilder.DropIndex(
                name: "IX_LessonResults_CreatedUserId",
                table: "LessonResults");

            migrationBuilder.DropIndex(
                name: "IX_CourseResults_CreatedUserId",
                table: "CourseResults");

            migrationBuilder.CreateIndex(
                name: "IX_VideoTimeCodeAnswers_QuestionId",
                table: "VideoTimeCodeAnswers",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_UnitResults_CreatedUserId",
                table: "UnitResults",
                column: "CreatedUserId");
        }
    }
}
