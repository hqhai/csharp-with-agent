using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_Key_Unique_IsDeleted : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_VideoTimeCodeResults_VideoResultId_VideoTimeCodeId_StudentId",
                table: "VideoTimeCodeResults");

            migrationBuilder.DropIndex(
                name: "IX_VideoTimeCodeAnswers_VideoResultId_QuestionId_VideoTimeCodeResultId",
                table: "VideoTimeCodeAnswers");

            migrationBuilder.DropIndex(
                name: "IX_VideoResults_LessonResultId_VideoId_StudentId",
                table: "VideoResults");

            migrationBuilder.DropIndex(
                name: "IX_UnitResults_CourseId_UnitId_StudentId",
                table: "UnitResults");

            migrationBuilder.DropIndex(
                name: "IX_SectionGroupResults_SectionGroupId_FinalTestResultId",
                table: "SectionGroupResults");

            migrationBuilder.DropIndex(
                name: "IX_SectionGroupResults_SectionGroupId_MockTestResultId",
                table: "SectionGroupResults");

            migrationBuilder.DropIndex(
                name: "IX_SectionGroupResults_SectionGroupId_PlacementTestResultId",
                table: "SectionGroupResults");

            migrationBuilder.DropIndex(
                name: "IX_QuestionShuffles_StudentId_QuestionId",
                table: "QuestionShuffles");

            migrationBuilder.DropIndex(
                name: "IX_PlacementTestResults_PlacementTestId_StudentId",
                table: "PlacementTestResults");

            migrationBuilder.DropIndex(
                name: "IX_PlacementTestGroupResults_StudentId",
                table: "PlacementTestGroupResults");

            migrationBuilder.DropIndex(
                name: "IX_PlacementTestAnswers_PlacementTestResultId_SectionGroupResultId_SectionQuestionId",
                table: "PlacementTestAnswers");

            migrationBuilder.DropIndex(
                name: "IX_MockTestResults_CourseId_MockTestId_StudentId",
                table: "MockTestResults");

            migrationBuilder.DropIndex(
                name: "IX_MockTestAnswers_MockTestResultId_SectionGroupResultId_SectionId",
                table: "MockTestAnswers");

            migrationBuilder.DropIndex(
                name: "IX_MockTestAnswers_MockTestResultId_SectionGroupResultId_SectionQuestionId",
                table: "MockTestAnswers");

            migrationBuilder.DropIndex(
                name: "IX_MockTestAnswers_MockTestResultId_SectionGroupResultId_SectionTimeCodeId",
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
                name: "IX_VideoTimeCodeResults_VideoResultId_VideoTimeCodeId_StudentId",
                table: "VideoTimeCodeResults",
                columns: new[] { "VideoResultId", "VideoTimeCodeId", "StudentId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_VideoTimeCodeAnswers_VideoResultId_QuestionId_VideoTimeCodeResultId",
                table: "VideoTimeCodeAnswers",
                columns: new[] { "VideoResultId", "QuestionId", "VideoTimeCodeResultId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_VideoResults_LessonResultId_VideoId_StudentId",
                table: "VideoResults",
                columns: new[] { "LessonResultId", "VideoId", "StudentId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_UnitResults_CourseId_UnitId_StudentId",
                table: "UnitResults",
                columns: new[] { "CourseId", "UnitId", "StudentId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_SectionGroupResults_SectionGroupId_FinalTestResultId",
                table: "SectionGroupResults",
                columns: new[] { "SectionGroupId", "FinalTestResultId" },
                unique: true,
                filter: "FinalTestResultId IS NOT NULL AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_SectionGroupResults_SectionGroupId_MockTestResultId",
                table: "SectionGroupResults",
                columns: new[] { "SectionGroupId", "MockTestResultId" },
                unique: true,
                filter: "MockTestResultId IS NOT NULL AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_SectionGroupResults_SectionGroupId_PlacementTestResultId",
                table: "SectionGroupResults",
                columns: new[] { "SectionGroupId", "PlacementTestResultId" },
                unique: true,
                filter: "PlacementTestResultId IS NOT NULL AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionShuffles_StudentId_QuestionId",
                table: "QuestionShuffles",
                columns: new[] { "StudentId", "QuestionId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_PlacementTestResults_PlacementTestId_StudentId",
                table: "PlacementTestResults",
                columns: new[] { "PlacementTestId", "StudentId" },
                unique: true,
                filter: "PlacementTestId IS NOT NULL AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_PlacementTestGroupResults_StudentId",
                table: "PlacementTestGroupResults",
                column: "StudentId",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_PlacementTestAnswers_PlacementTestResultId_SectionGroupResultId_SectionQuestionId",
                table: "PlacementTestAnswers",
                columns: new[] { "PlacementTestResultId", "SectionGroupResultId", "SectionQuestionId" },
                unique: true,
                filter: "SectionGroupResultId IS NOT NULL AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_MockTestResults_CourseId_MockTestId_StudentId",
                table: "MockTestResults",
                columns: new[] { "CourseId", "MockTestId", "StudentId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_MockTestAnswers_MockTestResultId_SectionGroupResultId_SectionId",
                table: "MockTestAnswers",
                columns: new[] { "MockTestResultId", "SectionGroupResultId", "SectionId" },
                unique: true,
                filter: "SectionId IS NOT NULL AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_MockTestAnswers_MockTestResultId_SectionGroupResultId_SectionQuestionId",
                table: "MockTestAnswers",
                columns: new[] { "MockTestResultId", "SectionGroupResultId", "SectionQuestionId" },
                unique: true,
                filter: "SectionQuestionId IS NOT NULL AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_MockTestAnswers_MockTestResultId_SectionGroupResultId_SectionTimeCodeId",
                table: "MockTestAnswers",
                columns: new[] { "MockTestResultId", "SectionGroupResultId", "SectionTimeCodeId" },
                unique: true,
                filter: "SectionTimeCodeId IS NOT NULL AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_LessonResults_CourseId_UnitId_LessonId_StudentId",
                table: "LessonResults",
                columns: new[] { "CourseId", "UnitId", "LessonId", "StudentId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_HomeWorkResults_LessonResultId_HomeWorkId_StudentId",
                table: "HomeWorkResults",
                columns: new[] { "LessonResultId", "HomeWorkId", "StudentId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_HomeWorkAnswers_HomeWorkQuestionId_HomeWorkResultId",
                table: "HomeWorkAnswers",
                columns: new[] { "HomeWorkQuestionId", "HomeWorkResultId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_FinalTestResults_CourseId_FinalTestId_StudentId",
                table: "FinalTestResults",
                columns: new[] { "CourseId", "FinalTestId", "StudentId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_FinalTestAnswers_FinalTestResultId_SectionQuestionId_SectionGroupResultId",
                table: "FinalTestAnswers",
                columns: new[] { "FinalTestResultId", "SectionQuestionId", "SectionGroupResultId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Courses_ParentCourseId_Priority",
                table: "Courses",
                columns: new[] { "ParentCourseId", "Priority" },
                unique: true,
                filter: "ParentCourseId IS NOT NULL AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_CourseResults_CourseId_StudentId",
                table: "CourseResults",
                columns: new[] { "CourseId", "StudentId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_ClassForumResults_LessonResultId_ClassForumId_StudentId",
                table: "ClassForumResults",
                columns: new[] { "LessonResultId", "ClassForumId", "StudentId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_ClassForumDetailResults_ClassForumResultId_SubmissionCount",
                table: "ClassForumDetailResults",
                columns: new[] { "ClassForumResultId", "SubmissionCount" },
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_VideoTimeCodeResults_VideoResultId_VideoTimeCodeId_StudentId",
                table: "VideoTimeCodeResults");

            migrationBuilder.DropIndex(
                name: "IX_VideoTimeCodeAnswers_VideoResultId_QuestionId_VideoTimeCodeResultId",
                table: "VideoTimeCodeAnswers");

            migrationBuilder.DropIndex(
                name: "IX_VideoResults_LessonResultId_VideoId_StudentId",
                table: "VideoResults");

            migrationBuilder.DropIndex(
                name: "IX_UnitResults_CourseId_UnitId_StudentId",
                table: "UnitResults");

            migrationBuilder.DropIndex(
                name: "IX_SectionGroupResults_SectionGroupId_FinalTestResultId",
                table: "SectionGroupResults");

            migrationBuilder.DropIndex(
                name: "IX_SectionGroupResults_SectionGroupId_MockTestResultId",
                table: "SectionGroupResults");

            migrationBuilder.DropIndex(
                name: "IX_SectionGroupResults_SectionGroupId_PlacementTestResultId",
                table: "SectionGroupResults");

            migrationBuilder.DropIndex(
                name: "IX_QuestionShuffles_StudentId_QuestionId",
                table: "QuestionShuffles");

            migrationBuilder.DropIndex(
                name: "IX_PlacementTestResults_PlacementTestId_StudentId",
                table: "PlacementTestResults");

            migrationBuilder.DropIndex(
                name: "IX_PlacementTestGroupResults_StudentId",
                table: "PlacementTestGroupResults");

            migrationBuilder.DropIndex(
                name: "IX_PlacementTestAnswers_PlacementTestResultId_SectionGroupResultId_SectionQuestionId",
                table: "PlacementTestAnswers");

            migrationBuilder.DropIndex(
                name: "IX_MockTestResults_CourseId_MockTestId_StudentId",
                table: "MockTestResults");

            migrationBuilder.DropIndex(
                name: "IX_MockTestAnswers_MockTestResultId_SectionGroupResultId_SectionId",
                table: "MockTestAnswers");

            migrationBuilder.DropIndex(
                name: "IX_MockTestAnswers_MockTestResultId_SectionGroupResultId_SectionQuestionId",
                table: "MockTestAnswers");

            migrationBuilder.DropIndex(
                name: "IX_MockTestAnswers_MockTestResultId_SectionGroupResultId_SectionTimeCodeId",
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
                name: "IX_VideoTimeCodeResults_VideoResultId_VideoTimeCodeId_StudentId",
                table: "VideoTimeCodeResults",
                columns: new[] { "VideoResultId", "VideoTimeCodeId", "StudentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VideoTimeCodeAnswers_VideoResultId_QuestionId_VideoTimeCodeResultId",
                table: "VideoTimeCodeAnswers",
                columns: new[] { "VideoResultId", "QuestionId", "VideoTimeCodeResultId" },
                unique: true,
                filter: "[VideoResultId] IS NOT NULL AND [VideoTimeCodeResultId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_VideoResults_LessonResultId_VideoId_StudentId",
                table: "VideoResults",
                columns: new[] { "LessonResultId", "VideoId", "StudentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UnitResults_CourseId_UnitId_StudentId",
                table: "UnitResults",
                columns: new[] { "CourseId", "UnitId", "StudentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SectionGroupResults_SectionGroupId_FinalTestResultId",
                table: "SectionGroupResults",
                columns: new[] { "SectionGroupId", "FinalTestResultId" },
                unique: true,
                filter: "[FinalTestResultId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SectionGroupResults_SectionGroupId_MockTestResultId",
                table: "SectionGroupResults",
                columns: new[] { "SectionGroupId", "MockTestResultId" },
                unique: true,
                filter: "[MockTestResultId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SectionGroupResults_SectionGroupId_PlacementTestResultId",
                table: "SectionGroupResults",
                columns: new[] { "SectionGroupId", "PlacementTestResultId" },
                unique: true,
                filter: "[PlacementTestResultId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionShuffles_StudentId_QuestionId",
                table: "QuestionShuffles",
                columns: new[] { "StudentId", "QuestionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlacementTestResults_PlacementTestId_StudentId",
                table: "PlacementTestResults",
                columns: new[] { "PlacementTestId", "StudentId" },
                unique: true,
                filter: "[PlacementTestId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PlacementTestGroupResults_StudentId",
                table: "PlacementTestGroupResults",
                column: "StudentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlacementTestAnswers_PlacementTestResultId_SectionGroupResultId_SectionQuestionId",
                table: "PlacementTestAnswers",
                columns: new[] { "PlacementTestResultId", "SectionGroupResultId", "SectionQuestionId" },
                unique: true,
                filter: "[SectionGroupResultId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MockTestResults_CourseId_MockTestId_StudentId",
                table: "MockTestResults",
                columns: new[] { "CourseId", "MockTestId", "StudentId" },
                unique: true,
                filter: "[UnitId] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MockTestAnswers_MockTestResultId_SectionGroupResultId_SectionId",
                table: "MockTestAnswers",
                columns: new[] { "MockTestResultId", "SectionGroupResultId", "SectionId" },
                unique: true,
                filter: "[SectionGroupResultId] IS NOT NULL AND [SectionId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MockTestAnswers_MockTestResultId_SectionGroupResultId_SectionQuestionId",
                table: "MockTestAnswers",
                columns: new[] { "MockTestResultId", "SectionGroupResultId", "SectionQuestionId" },
                unique: true,
                filter: "[SectionGroupResultId] IS NOT NULL AND [SectionQuestionId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MockTestAnswers_MockTestResultId_SectionGroupResultId_SectionTimeCodeId",
                table: "MockTestAnswers",
                columns: new[] { "MockTestResultId", "SectionGroupResultId", "SectionTimeCodeId" },
                unique: true,
                filter: "[SectionGroupResultId] IS NOT NULL AND [SectionTimeCodeId] IS NOT NULL");

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
    }
}
