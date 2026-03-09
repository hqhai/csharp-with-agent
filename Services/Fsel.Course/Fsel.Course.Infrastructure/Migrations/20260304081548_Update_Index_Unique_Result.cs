using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_Index_Unique_Result : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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
                name: "IX_VideoResults_StudentId",
                table: "VideoResults");

            migrationBuilder.DropIndex(
                name: "IX_UnitResults_CourseResultId_CourseModuleId_IsDeleted",
                table: "UnitResults");

            migrationBuilder.DropIndex(
                name: "IX_TestSectionResult_TestResultId",
                table: "TestSectionResult");

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
                name: "IX_LessonResults_CreatedUserId",
                table: "LessonResults");

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

            migrationBuilder.DropIndex(
                name: "IX_ClassForumDetailResults_ClassForumResultId",
                table: "ClassForumDetailResults");

            migrationBuilder.DropIndex(
                name: "IX_ClassForumDetailResults_IsDeleted_ClassForumResultId",
                table: "ClassForumDetailResults");

            migrationBuilder.DropIndex(
                name: "IX_ClassForumDetailResults_IsDeleted_Status_ClassForumResultId",
                table: "ClassForumDetailResults");

            migrationBuilder.CreateIndex(
                name: "IX_VideoTimeCodeResults_VideoResultId_VideoTimeCodeId",
                table: "VideoTimeCodeResults",
                columns: new[] { "VideoResultId", "VideoTimeCodeId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_VideoTimeCodeAnswers_VideoTimeCodeResultId_QuestionId",
                table: "VideoTimeCodeAnswers",
                columns: new[] { "VideoTimeCodeResultId", "QuestionId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_VideoResults_LessonModuleId_LessonResultId",
                table: "VideoResults",
                columns: new[] { "LessonModuleId", "LessonResultId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_UnitResults_CourseResultId_CourseModuleId",
                table: "UnitResults",
                columns: new[] { "CourseResultId", "CourseModuleId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_TestSectionResult_TestResultId_TestSectionId",
                table: "TestSectionResult",
                columns: new[] { "TestResultId", "TestSectionId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_TestResult_TestGroupResultId_TestId",
                table: "TestResult",
                columns: new[] { "TestGroupResultId", "TestId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_TestGroupResult_CourseModuleId_CourseResultId",
                table: "TestGroupResult",
                columns: new[] { "CourseModuleId", "CourseResultId" },
                unique: true,
                filter: "[IsDeleted] = 0 AND [UnitResultId] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TestGroupResult_UnitModuleId_UnitResultId",
                table: "TestGroupResult",
                columns: new[] { "UnitModuleId", "UnitResultId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_TestAnswer_TestSectionResultId_TestSectionId_QuestionId",
                table: "TestAnswer",
                columns: new[] { "TestSectionResultId", "TestSectionId", "QuestionId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionShuffles_StudentId_QuestionId",
                table: "QuestionShuffles",
                columns: new[] { "StudentId", "QuestionId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_LessonResults_UnitModuleId_UnitResultId",
                table: "LessonResults",
                columns: new[] { "UnitModuleId", "UnitResultId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_HomeWorkResults_LessonResultId_LessonModuleId",
                table: "HomeWorkResults",
                columns: new[] { "LessonResultId", "LessonModuleId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_HomeWorkExtraPracticeResults_StudentId_HomeWorkId_HomeWorkRetryId",
                table: "HomeWorkExtraPracticeResults",
                columns: new[] { "StudentId", "HomeWorkId", "HomeWorkRetryId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_HomeWorkExtraPracticeAnswers_QuestionId_HomeWorkExtraPracticeResultId",
                table: "HomeWorkExtraPracticeAnswers",
                columns: new[] { "QuestionId", "HomeWorkExtraPracticeResultId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_HomeWorkAnswers_HomeWorkQuestionId_HomeWorkResultId",
                table: "HomeWorkAnswers",
                columns: new[] { "HomeWorkQuestionId", "HomeWorkResultId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentResults_LessonResultId_LessonModuleId",
                table: "DocumentResults",
                columns: new[] { "LessonResultId", "LessonModuleId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_CourseResults_CourseId_StudentId",
                table: "CourseResults",
                columns: new[] { "CourseId", "StudentId" },
                unique: true,
                filter: "[IsDeleted] = 0 AND [WorkingStatus] != 'NotWorking'");

            migrationBuilder.CreateIndex(
                name: "IX_ClassForumResults_LessonResultId_LessonModuleId",
                table: "ClassForumResults",
                columns: new[] { "LessonResultId", "LessonModuleId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_ClassForumDetailResults_ClassForumResultId_IsDeleted",
                table: "ClassForumDetailResults",
                columns: new[] { "ClassForumResultId", "IsDeleted" });

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
                name: "IX_VideoTimeCodeResults_VideoResultId_VideoTimeCodeId",
                table: "VideoTimeCodeResults");

            migrationBuilder.DropIndex(
                name: "IX_VideoTimeCodeAnswers_VideoTimeCodeResultId_QuestionId",
                table: "VideoTimeCodeAnswers");

            migrationBuilder.DropIndex(
                name: "IX_VideoResults_LessonModuleId_LessonResultId",
                table: "VideoResults");

            migrationBuilder.DropIndex(
                name: "IX_UnitResults_CourseResultId_CourseModuleId",
                table: "UnitResults");

            migrationBuilder.DropIndex(
                name: "IX_TestSectionResult_TestResultId_TestSectionId",
                table: "TestSectionResult");

            migrationBuilder.DropIndex(
                name: "IX_TestResult_TestGroupResultId_TestId",
                table: "TestResult");

            migrationBuilder.DropIndex(
                name: "IX_TestGroupResult_CourseModuleId_CourseResultId",
                table: "TestGroupResult");

            migrationBuilder.DropIndex(
                name: "IX_TestGroupResult_UnitModuleId_UnitResultId",
                table: "TestGroupResult");

            migrationBuilder.DropIndex(
                name: "IX_TestAnswer_TestSectionResultId_TestSectionId_QuestionId",
                table: "TestAnswer");

            migrationBuilder.DropIndex(
                name: "IX_QuestionShuffles_StudentId_QuestionId",
                table: "QuestionShuffles");

            migrationBuilder.DropIndex(
                name: "IX_LessonResults_UnitModuleId_UnitResultId",
                table: "LessonResults");

            migrationBuilder.DropIndex(
                name: "IX_HomeWorkResults_LessonResultId_LessonModuleId",
                table: "HomeWorkResults");

            migrationBuilder.DropIndex(
                name: "IX_HomeWorkExtraPracticeResults_StudentId_HomeWorkId_HomeWorkRetryId",
                table: "HomeWorkExtraPracticeResults");

            migrationBuilder.DropIndex(
                name: "IX_HomeWorkExtraPracticeAnswers_QuestionId_HomeWorkExtraPracticeResultId",
                table: "HomeWorkExtraPracticeAnswers");

            migrationBuilder.DropIndex(
                name: "IX_HomeWorkAnswers_HomeWorkQuestionId_HomeWorkResultId",
                table: "HomeWorkAnswers");

            migrationBuilder.DropIndex(
                name: "IX_DocumentResults_LessonResultId_LessonModuleId",
                table: "DocumentResults");

            migrationBuilder.DropIndex(
                name: "IX_CourseResults_CourseId_StudentId",
                table: "CourseResults");

            migrationBuilder.DropIndex(
                name: "IX_ClassForumResults_LessonResultId_LessonModuleId",
                table: "ClassForumResults");

            migrationBuilder.DropIndex(
                name: "IX_ClassForumDetailResults_ClassForumResultId_IsDeleted",
                table: "ClassForumDetailResults");

            migrationBuilder.DropIndex(
                name: "IX_ClassForumDetailResults_ClassForumResultId_SubmissionCount",
                table: "ClassForumDetailResults");

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
                name: "IX_VideoResults_StudentId",
                table: "VideoResults",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_UnitResults_CourseResultId_CourseModuleId_IsDeleted",
                table: "UnitResults",
                columns: new[] { "CourseResultId", "CourseModuleId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_TestSectionResult_TestResultId",
                table: "TestSectionResult",
                column: "TestResultId");

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
                name: "IX_LessonResults_CreatedUserId",
                table: "LessonResults",
                column: "CreatedUserId")
                .Annotation("SqlServer:Include", new[] { "CompletionDate", "CorrectCount", "CorrectTotal", "CourseId", "CourseResultId", "CreatedDate", "CreatedFullName", "IsDeleted", "LessonId", "NewDate", "Percent", "PercentModule", "ProcessDate", "SkillScoresStr", "Status", "StudentId", "SummaryNote", "UnitId", "UnitModuleId", "UnitResultId", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" });

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

            migrationBuilder.CreateIndex(
                name: "IX_ClassForumDetailResults_ClassForumResultId",
                table: "ClassForumDetailResults",
                column: "ClassForumResultId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassForumDetailResults_IsDeleted_ClassForumResultId",
                table: "ClassForumDetailResults",
                columns: new[] { "IsDeleted", "ClassForumResultId" });

            migrationBuilder.CreateIndex(
                name: "IX_ClassForumDetailResults_IsDeleted_Status_ClassForumResultId",
                table: "ClassForumDetailResults",
                columns: new[] { "IsDeleted", "Status", "ClassForumResultId" });
        }
    }
}
