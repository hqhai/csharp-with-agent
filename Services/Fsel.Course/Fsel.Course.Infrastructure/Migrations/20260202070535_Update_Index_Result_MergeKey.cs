using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_Index_Result_MergeKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_VideoTimeCodeAnswers_VideoTimeCodeResultId",
                table: "VideoTimeCodeAnswers");

            migrationBuilder.DropIndex(
                name: "IX_VideoResults_LessonModuleId",
                table: "VideoResults");

            migrationBuilder.DropIndex(
                name: "IX_UnitResults_CourseResultId",
                table: "UnitResults");

            migrationBuilder.DropIndex(
                name: "IX_TestResult_TestGroupResultId",
                table: "TestResult");

            migrationBuilder.DropIndex(
                name: "IX_TestGroupResult_CourseModuleId",
                table: "TestGroupResult");

            migrationBuilder.DropIndex(
                name: "IX_TestGroupResult_UnitModuleId",
                table: "TestGroupResult");

            migrationBuilder.DropIndex(
                name: "IX_TestAnswer_TestSectionResultId",
                table: "TestAnswer");

            migrationBuilder.DropIndex(
                name: "IX_LessonResults_UnitModuleId",
                table: "LessonResults");

            migrationBuilder.DropIndex(
                name: "IX_HomeWorkResults_LessonResultId",
                table: "HomeWorkResults");

            migrationBuilder.DropIndex(
                name: "IX_HomeWorkExtraPracticeAnswers_QuestionId",
                table: "HomeWorkExtraPracticeAnswers");

            migrationBuilder.DropIndex(
                name: "IX_HomeWorkAnswers_HomeWorkQuestionId",
                table: "HomeWorkAnswers");

            migrationBuilder.DropIndex(
                name: "IX_DocumentResults_LessonResultId",
                table: "DocumentResults");

            migrationBuilder.DropIndex(
                name: "IX_CourseResults_CourseId",
                table: "CourseResults");

            migrationBuilder.DropIndex(
                name: "IX_ClassForumResults_LessonResultId",
                table: "ClassForumResults");

            migrationBuilder.CreateIndex(
                name: "IX_VideoTimeCodeResults_VideoResultId_VideoTimeCodeId_IsDeleted",
                table: "VideoTimeCodeResults",
                columns: new[] { "VideoResultId", "VideoTimeCodeId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_VideoTimeCodeAnswers_VideoTimeCodeResultId_QuestionId_IsDeleted",
                table: "VideoTimeCodeAnswers",
                columns: new[] { "VideoTimeCodeResultId", "QuestionId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_VideoResults_LessonModuleId_LessonResultId_IsDeleted",
                table: "VideoResults",
                columns: new[] { "LessonModuleId", "LessonResultId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_UnitResults_CourseResultId_CourseModuleId_IsDeleted",
                table: "UnitResults",
                columns: new[] { "CourseResultId", "CourseModuleId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_TestResult_TestGroupResultId_TestId_IsDeleted",
                table: "TestResult",
                columns: new[] { "TestGroupResultId", "TestId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_TestGroupResult_CourseModuleId_CourseResultId_IsDeleted",
                table: "TestGroupResult",
                columns: new[] { "CourseModuleId", "CourseResultId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_TestGroupResult_UnitModuleId_UnitResultId_IsDeleted",
                table: "TestGroupResult",
                columns: new[] { "UnitModuleId", "UnitResultId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_TestAnswer_TestSectionResultId_TestSectionId_QuestionId_IsDeleted",
                table: "TestAnswer",
                columns: new[] { "TestSectionResultId", "TestSectionId", "QuestionId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_LessonResults_UnitModuleId_UnitResultId_IsDeleted",
                table: "LessonResults",
                columns: new[] { "UnitModuleId", "UnitResultId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_HomeWorkResults_LessonResultId_LessonModuleId_IsDeleted",
                table: "HomeWorkResults",
                columns: new[] { "LessonResultId", "LessonModuleId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_HomeWorkExtraPracticeResults_StudentId_HomeWorkId_HomeWorkRetryId_WorkingStatus_IsDeleted",
                table: "HomeWorkExtraPracticeResults",
                columns: new[] { "StudentId", "HomeWorkId", "HomeWorkRetryId", "WorkingStatus", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_HomeWorkExtraPracticeAnswers_QuestionId_HomeWorkExtraPracticeResultId_IsDeleted",
                table: "HomeWorkExtraPracticeAnswers",
                columns: new[] { "QuestionId", "HomeWorkExtraPracticeResultId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_HomeWorkAnswers_HomeWorkQuestionId_HomeWorkResultId_IsDeleted",
                table: "HomeWorkAnswers",
                columns: new[] { "HomeWorkQuestionId", "HomeWorkResultId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentResults_LessonResultId_LessonModuleId_IsDeleted",
                table: "DocumentResults",
                columns: new[] { "LessonResultId", "LessonModuleId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_CourseResults_CourseId_StudentId_IsDeleted_WorkingStatus",
                table: "CourseResults",
                columns: new[] { "CourseId", "StudentId", "IsDeleted", "WorkingStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_ClassForumResults_LessonResultId_LessonModuleId_IsDeleted",
                table: "ClassForumResults",
                columns: new[] { "LessonResultId", "LessonModuleId", "IsDeleted" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_VideoTimeCodeResults_VideoResultId_VideoTimeCodeId_IsDeleted",
                table: "VideoTimeCodeResults");

            migrationBuilder.DropIndex(
                name: "IX_VideoTimeCodeAnswers_VideoTimeCodeResultId_QuestionId_IsDeleted",
                table: "VideoTimeCodeAnswers");

            migrationBuilder.DropIndex(
                name: "IX_VideoResults_LessonModuleId_LessonResultId_IsDeleted",
                table: "VideoResults");

            migrationBuilder.DropIndex(
                name: "IX_UnitResults_CourseResultId_CourseModuleId_IsDeleted",
                table: "UnitResults");

            migrationBuilder.DropIndex(
                name: "IX_TestResult_TestGroupResultId_TestId_IsDeleted",
                table: "TestResult");

            migrationBuilder.DropIndex(
                name: "IX_TestGroupResult_CourseModuleId_CourseResultId_IsDeleted",
                table: "TestGroupResult");

            migrationBuilder.DropIndex(
                name: "IX_TestGroupResult_UnitModuleId_UnitResultId_IsDeleted",
                table: "TestGroupResult");

            migrationBuilder.DropIndex(
                name: "IX_TestAnswer_TestSectionResultId_TestSectionId_QuestionId_IsDeleted",
                table: "TestAnswer");

            migrationBuilder.DropIndex(
                name: "IX_LessonResults_UnitModuleId_UnitResultId_IsDeleted",
                table: "LessonResults");

            migrationBuilder.DropIndex(
                name: "IX_HomeWorkResults_LessonResultId_LessonModuleId_IsDeleted",
                table: "HomeWorkResults");

            migrationBuilder.DropIndex(
                name: "IX_HomeWorkExtraPracticeResults_StudentId_HomeWorkId_HomeWorkRetryId_WorkingStatus_IsDeleted",
                table: "HomeWorkExtraPracticeResults");

            migrationBuilder.DropIndex(
                name: "IX_HomeWorkExtraPracticeAnswers_QuestionId_HomeWorkExtraPracticeResultId_IsDeleted",
                table: "HomeWorkExtraPracticeAnswers");

            migrationBuilder.DropIndex(
                name: "IX_HomeWorkAnswers_HomeWorkQuestionId_HomeWorkResultId_IsDeleted",
                table: "HomeWorkAnswers");

            migrationBuilder.DropIndex(
                name: "IX_DocumentResults_LessonResultId_LessonModuleId_IsDeleted",
                table: "DocumentResults");

            migrationBuilder.DropIndex(
                name: "IX_CourseResults_CourseId_StudentId_IsDeleted_WorkingStatus",
                table: "CourseResults");

            migrationBuilder.DropIndex(
                name: "IX_ClassForumResults_LessonResultId_LessonModuleId_IsDeleted",
                table: "ClassForumResults");

            migrationBuilder.CreateIndex(
                name: "IX_VideoTimeCodeAnswers_VideoTimeCodeResultId",
                table: "VideoTimeCodeAnswers",
                column: "VideoTimeCodeResultId");

            migrationBuilder.CreateIndex(
                name: "IX_VideoResults_LessonModuleId",
                table: "VideoResults",
                column: "LessonModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_UnitResults_CourseResultId",
                table: "UnitResults",
                column: "CourseResultId");

            migrationBuilder.CreateIndex(
                name: "IX_TestResult_TestGroupResultId",
                table: "TestResult",
                column: "TestGroupResultId");

            migrationBuilder.CreateIndex(
                name: "IX_TestGroupResult_CourseModuleId",
                table: "TestGroupResult",
                column: "CourseModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_TestGroupResult_UnitModuleId",
                table: "TestGroupResult",
                column: "UnitModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_TestAnswer_TestSectionResultId",
                table: "TestAnswer",
                column: "TestSectionResultId");

            migrationBuilder.CreateIndex(
                name: "IX_LessonResults_UnitModuleId",
                table: "LessonResults",
                column: "UnitModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_HomeWorkResults_LessonResultId",
                table: "HomeWorkResults",
                column: "LessonResultId");

            migrationBuilder.CreateIndex(
                name: "IX_HomeWorkExtraPracticeAnswers_QuestionId",
                table: "HomeWorkExtraPracticeAnswers",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_HomeWorkAnswers_HomeWorkQuestionId",
                table: "HomeWorkAnswers",
                column: "HomeWorkQuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentResults_LessonResultId",
                table: "DocumentResults",
                column: "LessonResultId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseResults_CourseId",
                table: "CourseResults",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassForumResults_LessonResultId",
                table: "ClassForumResults",
                column: "LessonResultId");
        }
    }
}
