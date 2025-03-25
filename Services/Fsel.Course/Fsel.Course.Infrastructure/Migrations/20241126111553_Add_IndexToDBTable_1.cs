using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_IndexToDBTable_1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_VideoTimeCodes_IsDeleted_TimeCodeType_VideoId",
                table: "VideoTimeCodes",
                columns: new[] { "IsDeleted", "TimeCodeType", "VideoId" });

            migrationBuilder.CreateIndex(
                name: "IX_VideoTimeCodes_IsDeleted_VideoId",
                table: "VideoTimeCodes",
                columns: new[] { "IsDeleted", "VideoId" });

            migrationBuilder.CreateIndex(
                name: "IX_VideoResults_Status_StudentId",
                table: "VideoResults",
                columns: new[] { "Status", "StudentId" });

            migrationBuilder.CreateIndex(
                name: "IX_VideoResults_StudentId",
                table: "VideoResults",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_UnitResults_CreatedUserId",
                table: "UnitResults",
                column: "CreatedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UnitResults_StudentId_Status",
                table: "UnitResults",
                columns: new[] { "StudentId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_StudentFeedbacks_IsDeleted_Type_ObjectId",
                table: "StudentFeedbacks",
                columns: new[] { "IsDeleted", "Type", "ObjectId" });

            migrationBuilder.CreateIndex(
                name: "IX_SectionGroupResults_StudentId",
                table: "SectionGroupResults",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_PlacementTestAnswers_PlacementTestResultId_SectionQuestionId",
                table: "PlacementTestAnswers",
                columns: new[] { "PlacementTestResultId", "SectionQuestionId" });

            migrationBuilder.CreateIndex(
                name: "IX_MockTestResults_StudentId",
                table: "MockTestResults",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_LessonResults_CreatedUserId_Status_UnitId",
                table: "LessonResults",
                columns: new[] { "CreatedUserId", "Status", "UnitId" });

            migrationBuilder.CreateIndex(
                name: "IX_HomeWorkResults_StudentId",
                table: "HomeWorkResults",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_FinalTestResults_StudentId_CourseId",
                table: "FinalTestResults",
                columns: new[] { "StudentId", "CourseId" });

            migrationBuilder.CreateIndex(
                name: "IX_CourseResults_StudentId_WorkingStatus",
                table: "CourseResults",
                columns: new[] { "StudentId", "WorkingStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_ClassForumResults_IsDeleted_StudentId",
                table: "ClassForumResults",
                columns: new[] { "IsDeleted", "StudentId" });

            migrationBuilder.CreateIndex(
                name: "IX_ClassForumDetailResults_IsDeleted_ClassForumResultId",
                table: "ClassForumDetailResults",
                columns: new[] { "IsDeleted", "ClassForumResultId" });

            migrationBuilder.CreateIndex(
                name: "IX_ClassForumDetailResults_IsDeleted_Status_ClassForumResultId",
                table: "ClassForumDetailResults",
                columns: new[] { "IsDeleted", "Status", "ClassForumResultId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_VideoTimeCodes_IsDeleted_TimeCodeType_VideoId",
                table: "VideoTimeCodes");

            migrationBuilder.DropIndex(
                name: "IX_VideoTimeCodes_IsDeleted_VideoId",
                table: "VideoTimeCodes");

            migrationBuilder.DropIndex(
                name: "IX_VideoResults_Status_StudentId",
                table: "VideoResults");

            migrationBuilder.DropIndex(
                name: "IX_VideoResults_StudentId",
                table: "VideoResults");

            migrationBuilder.DropIndex(
                name: "IX_UnitResults_CreatedUserId",
                table: "UnitResults");

            migrationBuilder.DropIndex(
                name: "IX_UnitResults_StudentId_Status",
                table: "UnitResults");

            migrationBuilder.DropIndex(
                name: "IX_StudentFeedbacks_IsDeleted_Type_ObjectId",
                table: "StudentFeedbacks");

            migrationBuilder.DropIndex(
                name: "IX_SectionGroupResults_StudentId",
                table: "SectionGroupResults");

            migrationBuilder.DropIndex(
                name: "IX_PlacementTestAnswers_PlacementTestResultId_SectionQuestionId",
                table: "PlacementTestAnswers");

            migrationBuilder.DropIndex(
                name: "IX_MockTestResults_StudentId",
                table: "MockTestResults");

            migrationBuilder.DropIndex(
                name: "IX_LessonResults_CreatedUserId_Status_UnitId",
                table: "LessonResults");

            migrationBuilder.DropIndex(
                name: "IX_HomeWorkResults_StudentId",
                table: "HomeWorkResults");

            migrationBuilder.DropIndex(
                name: "IX_FinalTestResults_StudentId_CourseId",
                table: "FinalTestResults");

            migrationBuilder.DropIndex(
                name: "IX_CourseResults_StudentId_WorkingStatus",
                table: "CourseResults");

            migrationBuilder.DropIndex(
                name: "IX_ClassForumResults_IsDeleted_StudentId",
                table: "ClassForumResults");

            migrationBuilder.DropIndex(
                name: "IX_ClassForumDetailResults_IsDeleted_ClassForumResultId",
                table: "ClassForumDetailResults");

            migrationBuilder.DropIndex(
                name: "IX_ClassForumDetailResults_IsDeleted_Status_ClassForumResultId",
                table: "ClassForumDetailResults");
        }
    }
}
