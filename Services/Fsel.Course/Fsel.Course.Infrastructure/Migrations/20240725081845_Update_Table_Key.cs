using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_Table_Key : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_VideoTimeCodeResults_VideoResultId",
                table: "VideoTimeCodeResults");

            migrationBuilder.DropIndex(
                name: "IX_VideoTimeCodeAnswers_VideoResultId",
                table: "VideoTimeCodeAnswers");

            migrationBuilder.DropIndex(
                name: "IX_SectionGroupResults_SectionGroupId",
                table: "SectionGroupResults");

            migrationBuilder.DropIndex(
                name: "IX_PlacementTestResults_PlacementTestId",
                table: "PlacementTestResults");

            migrationBuilder.DropIndex(
                name: "IX_PlacementTestAnswers_PlacementTestResultId",
                table: "PlacementTestAnswers");

            migrationBuilder.DropIndex(
                name: "IX_MockTestResults_CourseId",
                table: "MockTestResults");

            migrationBuilder.DropIndex(
                name: "IX_MockTestAnswers_MockTestResultId",
                table: "MockTestAnswers");

            migrationBuilder.DropIndex(
                name: "IX_LessonResults_CourseId",
                table: "LessonResults");

            migrationBuilder.DropIndex(
                name: "IX_HomeWorkResults_LessonResultId",
                table: "HomeWorkResults");

            migrationBuilder.DropIndex(
                name: "IX_HomeWorkAnswers_HomeWorkQuestionId",
                table: "HomeWorkAnswers");

            migrationBuilder.DropIndex(
                name: "IX_FinalTestResults_CourseId",
                table: "FinalTestResults");

            migrationBuilder.DropIndex(
                name: "IX_FinalTestAnswers_FinalTestResultId",
                table: "FinalTestAnswers");

            migrationBuilder.DropIndex(
                name: "IX_CourseResults_CourseId",
                table: "CourseResults");

            migrationBuilder.DropIndex(
                name: "IX_ClassForumResults_LessonResultId",
                table: "ClassForumResults");

            migrationBuilder.DropIndex(
                name: "IX_ClassForumDetailResults_ClassForumResultId",
                table: "ClassForumDetailResults");

            migrationBuilder.CreateIndex(
                name: "IX_VideoTimeCodeResults_VideoResultId_VideoTimeCodeId_StudentId",
                table: "VideoTimeCodeResults",
                columns: new[] { "VideoResultId", "VideoTimeCodeId", "StudentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VideoTimeCodeAnswers_VideoResultId_VideoTimeCodeId_ExerciseId_QuestionId_VideoTimeCodeResultId",
                table: "VideoTimeCodeAnswers",
                columns: new[] { "VideoResultId", "VideoTimeCodeId", "ExerciseId", "QuestionId", "VideoTimeCodeResultId" },
                unique: true,
                filter: "[VideoResultId] IS NOT NULL AND [VideoTimeCodeResultId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_VideoResults_LessonResultId_VideoId_StudentId",
                table: "VideoResults",
                columns: new[] { "LessonResultId", "VideoId", "StudentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SectionGroupResults_SectionGroupId_MockTestResultId_FinalTestResultId_PlacementTestResultId",
                table: "SectionGroupResults",
                columns: new[] { "SectionGroupId", "MockTestResultId", "FinalTestResultId", "PlacementTestResultId" },
                unique: true,
                filter: "[MockTestResultId] IS NOT NULL AND [FinalTestResultId] IS NOT NULL AND [PlacementTestResultId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PlacementTestResults_PlacementTestId_StudentId",
                table: "PlacementTestResults",
                columns: new[] { "PlacementTestId", "StudentId" },
                unique: true,
                filter: "[PlacementTestId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PlacementTestAnswers_PlacementTestResultId_SectionGroupResultId_SectionQuestionId",
                table: "PlacementTestAnswers",
                columns: new[] { "PlacementTestResultId", "SectionGroupResultId", "SectionQuestionId" },
                unique: true,
                filter: "[SectionGroupResultId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MockTestResults_CourseId_MockTestId_UnitId_StudentId",
                table: "MockTestResults",
                columns: new[] { "CourseId", "MockTestId", "UnitId", "StudentId" },
                unique: true,
                filter: "[UnitId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MockTestAnswers_MockTestResultId_SectionQuestionId_SectionGroupResultId_SectionTimeCodeId_SectionId",
                table: "MockTestAnswers",
                columns: new[] { "MockTestResultId", "SectionQuestionId", "SectionGroupResultId", "SectionTimeCodeId", "SectionId" },
                unique: true,
                filter: "[SectionQuestionId] IS NOT NULL AND [SectionGroupResultId] IS NOT NULL AND [SectionTimeCodeId] IS NOT NULL AND [SectionId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_LessonResults_CourseId_UnitId_LessonId_StudentId",
                table: "LessonResults",
                columns: new[] { "CourseId", "UnitId", "LessonId", "StudentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HomeWorkResults_LessonResultId_HomeWorkId_StudentId",
                table: "HomeWorkResults",
                columns: new[] { "LessonResultId", "HomeWorkId", "StudentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HomeWorkAnswers_HomeWorkQuestionId_HomeWorkResultId",
                table: "HomeWorkAnswers",
                columns: new[] { "HomeWorkQuestionId", "HomeWorkResultId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FinalTestResults_CourseId_FinalTestId_StudentId",
                table: "FinalTestResults",
                columns: new[] { "CourseId", "FinalTestId", "StudentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FinalTestAnswers_FinalTestResultId_SectionQuestionId_SectionGroupResultId",
                table: "FinalTestAnswers",
                columns: new[] { "FinalTestResultId", "SectionQuestionId", "SectionGroupResultId" },
                unique: true,
                filter: "[SectionGroupResultId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Courses_ParentCourseId_Priority",
                table: "Courses",
                columns: new[] { "ParentCourseId", "Priority" },
                unique: true,
                filter: "[ParentCourseId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CourseResults_CourseId_StudentId",
                table: "CourseResults",
                columns: new[] { "CourseId", "StudentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClassForumResults_LessonResultId_ClassForumId_StudentId",
                table: "ClassForumResults",
                columns: new[] { "LessonResultId", "ClassForumId", "StudentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClassForumDetailResults_ClassForumResultId_SubmissionCount",
                table: "ClassForumDetailResults",
                columns: new[] { "ClassForumResultId", "SubmissionCount" },
                unique: true,
                filter: "[SubmissionCount] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_VideoTimeCodeResults_VideoResultId_VideoTimeCodeId_StudentId",
                table: "VideoTimeCodeResults");

            migrationBuilder.DropIndex(
                name: "IX_VideoTimeCodeAnswers_VideoResultId_VideoTimeCodeId_ExerciseId_QuestionId_VideoTimeCodeResultId",
                table: "VideoTimeCodeAnswers");

            migrationBuilder.DropIndex(
                name: "IX_VideoResults_LessonResultId_VideoId_StudentId",
                table: "VideoResults");

            migrationBuilder.DropIndex(
                name: "IX_SectionGroupResults_SectionGroupId_MockTestResultId_FinalTestResultId_PlacementTestResultId",
                table: "SectionGroupResults");

            migrationBuilder.DropIndex(
                name: "IX_PlacementTestResults_PlacementTestId_StudentId",
                table: "PlacementTestResults");

            migrationBuilder.DropIndex(
                name: "IX_PlacementTestAnswers_PlacementTestResultId_SectionGroupResultId_SectionQuestionId",
                table: "PlacementTestAnswers");

            migrationBuilder.DropIndex(
                name: "IX_MockTestResults_CourseId_MockTestId_UnitId_StudentId",
                table: "MockTestResults");

            migrationBuilder.DropIndex(
                name: "IX_MockTestAnswers_MockTestResultId_SectionQuestionId_SectionGroupResultId_SectionTimeCodeId_SectionId",
                table: "MockTestAnswers");

            migrationBuilder.DropIndex(
                name: "IX_LessonResults_CourseId_UnitId_LessonId_StudentId",
                table: "LessonResults");

            migrationBuilder.DropIndex(
                name: "IX_HomeWorkResults_LessonResultId_HomeWorkId_StudentId",
                table: "HomeWorkResults");

            migrationBuilder.DropIndex(
                name: "IX_HomeWorkAnswers_HomeWorkQuestionId_HomeWorkResultId",
                table: "HomeWorkAnswers");

            migrationBuilder.DropIndex(
                name: "IX_FinalTestResults_CourseId_FinalTestId_StudentId",
                table: "FinalTestResults");

            migrationBuilder.DropIndex(
                name: "IX_FinalTestAnswers_FinalTestResultId_SectionQuestionId_SectionGroupResultId",
                table: "FinalTestAnswers");

            migrationBuilder.DropIndex(
                name: "IX_Courses_ParentCourseId_Priority",
                table: "Courses");

            migrationBuilder.DropIndex(
                name: "IX_CourseResults_CourseId_StudentId",
                table: "CourseResults");

            migrationBuilder.DropIndex(
                name: "IX_ClassForumResults_LessonResultId_ClassForumId_StudentId",
                table: "ClassForumResults");

            migrationBuilder.DropIndex(
                name: "IX_ClassForumDetailResults_ClassForumResultId_SubmissionCount",
                table: "ClassForumDetailResults");

            migrationBuilder.CreateIndex(
                name: "IX_VideoTimeCodeResults_VideoResultId",
                table: "VideoTimeCodeResults",
                column: "VideoResultId");

            migrationBuilder.CreateIndex(
                name: "IX_VideoTimeCodeAnswers_VideoResultId",
                table: "VideoTimeCodeAnswers",
                column: "VideoResultId");

            migrationBuilder.CreateIndex(
                name: "IX_SectionGroupResults_SectionGroupId",
                table: "SectionGroupResults",
                column: "SectionGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_PlacementTestResults_PlacementTestId",
                table: "PlacementTestResults",
                column: "PlacementTestId");

            migrationBuilder.CreateIndex(
                name: "IX_PlacementTestAnswers_PlacementTestResultId",
                table: "PlacementTestAnswers",
                column: "PlacementTestResultId");

            migrationBuilder.CreateIndex(
                name: "IX_MockTestResults_CourseId",
                table: "MockTestResults",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_MockTestAnswers_MockTestResultId",
                table: "MockTestAnswers",
                column: "MockTestResultId");

            migrationBuilder.CreateIndex(
                name: "IX_LessonResults_CourseId",
                table: "LessonResults",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_HomeWorkResults_LessonResultId",
                table: "HomeWorkResults",
                column: "LessonResultId");

            migrationBuilder.CreateIndex(
                name: "IX_HomeWorkAnswers_HomeWorkQuestionId",
                table: "HomeWorkAnswers",
                column: "HomeWorkQuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_FinalTestResults_CourseId",
                table: "FinalTestResults",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_FinalTestAnswers_FinalTestResultId",
                table: "FinalTestAnswers",
                column: "FinalTestResultId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseResults_CourseId",
                table: "CourseResults",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassForumResults_LessonResultId",
                table: "ClassForumResults",
                column: "LessonResultId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassForumDetailResults_ClassForumResultId",
                table: "ClassForumDetailResults",
                column: "ClassForumResultId");
        }
    }
}
