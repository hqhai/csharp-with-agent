using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_Result_UnIsUnique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UnitResults_UnitResults_UnitResultId",
                table: "UnitResults");

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
                name: "IX_UnitResults_CreatedUserId",
                table: "UnitResults");

            migrationBuilder.DropIndex(
                name: "IX_UnitResults_UnitResultId",
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
                name: "IX_MockTestResults_CourseId_MockTestId_UnitId_StudentId",
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
                name: "IX_ExtraPractices_MockTestId",
                table: "ExtraPractices");

            migrationBuilder.DropIndex(
                name: "IX_ExtraPractices_PlacementTestId",
                table: "ExtraPractices");

            migrationBuilder.DropIndex(
                name: "IX_ExtraPractices_VideoId",
                table: "ExtraPractices");

            migrationBuilder.DropIndex(
                name: "IX_Courses_ParentCourseId_Priority",
                table: "Courses");

            migrationBuilder.DropIndex(
                name: "IX_CourseResults_CourseId_StudentId",
                table: "CourseResults");

            migrationBuilder.DropIndex(
                name: "IX_ClassForums_LessonId",
                table: "ClassForums");

            migrationBuilder.DropIndex(
                name: "IX_ClassForumResults_LessonResultId_ClassForumId_StudentId",
                table: "ClassForumResults");

            migrationBuilder.DropIndex(
                name: "IX_ClassForumDetailResults_ClassForumResultId_SubmissionCount",
                table: "ClassForumDetailResults");

            migrationBuilder.DropColumn(
                name: "UnitResultId",
                table: "UnitResults");

            migrationBuilder.CreateIndex(
                name: "IX_VideoTimeCodeAnswers_VideoResultId",
                table: "VideoTimeCodeAnswers",
                column: "VideoResultId");

            migrationBuilder.CreateIndex(
                name: "IX_UnitResults_CourseId",
                table: "UnitResults",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_SectionGroupResults_SectionGroupId",
                table: "SectionGroupResults",
                column: "SectionGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_PlacementTestResults_PlacementTestId",
                table: "PlacementTestResults",
                column: "PlacementTestId");

            migrationBuilder.CreateIndex(
                name: "IX_MockTestResults_CourseId",
                table: "MockTestResults",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_MockTestAnswers_MockTestResultId",
                table: "MockTestAnswers",
                column: "MockTestResultId");

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
                name: "IX_ExtraPractices_MockTestId",
                table: "ExtraPractices",
                column: "MockTestId",
                unique: true,
                filter: "[MockTestId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ExtraPractices_PlacementTestId",
                table: "ExtraPractices",
                column: "PlacementTestId",
                unique: true,
                filter: "[PlacementTestId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ExtraPractices_VideoId",
                table: "ExtraPractices",
                column: "VideoId",
                unique: true,
                filter: "[VideoId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CourseResults_CourseId",
                table: "CourseResults",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassForums_LessonId",
                table: "ClassForums",
                column: "LessonId",
                unique: true,
                filter: "[LessonId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ClassForumResults_LessonResultId",
                table: "ClassForumResults",
                column: "LessonResultId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassForumDetailResults_ClassForumResultId",
                table: "ClassForumDetailResults",
                column: "ClassForumResultId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_VideoTimeCodeAnswers_VideoResultId",
                table: "VideoTimeCodeAnswers");

            migrationBuilder.DropIndex(
                name: "IX_UnitResults_CourseId",
                table: "UnitResults");

            migrationBuilder.DropIndex(
                name: "IX_SectionGroupResults_SectionGroupId",
                table: "SectionGroupResults");

            migrationBuilder.DropIndex(
                name: "IX_PlacementTestResults_PlacementTestId",
                table: "PlacementTestResults");

            migrationBuilder.DropIndex(
                name: "IX_MockTestResults_CourseId",
                table: "MockTestResults");

            migrationBuilder.DropIndex(
                name: "IX_MockTestAnswers_MockTestResultId",
                table: "MockTestAnswers");

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
                name: "IX_ExtraPractices_MockTestId",
                table: "ExtraPractices");

            migrationBuilder.DropIndex(
                name: "IX_ExtraPractices_PlacementTestId",
                table: "ExtraPractices");

            migrationBuilder.DropIndex(
                name: "IX_ExtraPractices_VideoId",
                table: "ExtraPractices");

            migrationBuilder.DropIndex(
                name: "IX_CourseResults_CourseId",
                table: "CourseResults");

            migrationBuilder.DropIndex(
                name: "IX_ClassForums_LessonId",
                table: "ClassForums");

            migrationBuilder.DropIndex(
                name: "IX_ClassForumResults_LessonResultId",
                table: "ClassForumResults");

            migrationBuilder.DropIndex(
                name: "IX_ClassForumDetailResults_ClassForumResultId",
                table: "ClassForumDetailResults");

            migrationBuilder.AddColumn<Guid>(
                name: "UnitResultId",
                table: "UnitResults",
                type: "uniqueidentifier",
                nullable: true);

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
                name: "IX_UnitResults_CreatedUserId",
                table: "UnitResults",
                column: "CreatedUserId")
                .Annotation("SqlServer:Include", new[] { "CompletionDate", "CorrectCount", "CorrectTotal", "CourseId", "CourseModuleId", "CourseResultId", "CreatedDate", "CreatedFullName", "IsDeleted", "NewDate", "Percent", "PercentModule", "ProcessDate", "SkillScoresStr", "Status", "StudentId", "UnitId", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" });

            migrationBuilder.CreateIndex(
                name: "IX_UnitResults_UnitResultId",
                table: "UnitResults",
                column: "UnitResultId");

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
                filter: "[UnitId] IS NULL AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_MockTestResults_CourseId_MockTestId_UnitId_StudentId",
                table: "MockTestResults",
                columns: new[] { "CourseId", "MockTestId", "UnitId", "StudentId" },
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
                name: "IX_ExtraPractices_MockTestId",
                table: "ExtraPractices",
                column: "MockTestId");

            migrationBuilder.CreateIndex(
                name: "IX_ExtraPractices_PlacementTestId",
                table: "ExtraPractices",
                column: "PlacementTestId");

            migrationBuilder.CreateIndex(
                name: "IX_ExtraPractices_VideoId",
                table: "ExtraPractices",
                column: "VideoId");

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
                name: "IX_ClassForums_LessonId",
                table: "ClassForums",
                column: "LessonId");

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

            migrationBuilder.AddForeignKey(
                name: "FK_UnitResults_UnitResults_UnitResultId",
                table: "UnitResults",
                column: "UnitResultId",
                principalTable: "UnitResults",
                principalColumn: "Id");
        }
    }
}
