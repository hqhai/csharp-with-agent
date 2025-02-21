using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_IndexToDBTable_3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LessonResults_LessonId",
                table: "LessonResults");

            migrationBuilder.DropIndex(
                name: "IX_HomeWorkResults_StudentId",
                table: "HomeWorkResults");

            migrationBuilder.CreateIndex(
                name: "IX_VideoTimeCodeResults_VideoResultId_Status",
                table: "VideoTimeCodeResults",
                columns: new[] { "VideoResultId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_StudentFeedbacks_IsDeleted_ObjectId",
                table: "StudentFeedbacks",
                columns: new[] { "IsDeleted", "ObjectId" });

            migrationBuilder.CreateIndex(
                name: "IX_PlacementTestGroupResults_CreatedDate",
                table: "PlacementTestGroupResults",
                column: "CreatedDate")
                .Annotation("SqlServer:Include", new[] { "StudentId" });

            migrationBuilder.CreateIndex(
                name: "IX_LessonResults_CourseId_StudentId_Status",
                table: "LessonResults",
                columns: new[] { "CourseId", "StudentId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_LessonResults_LessonId",
                table: "LessonResults",
                column: "LessonId")
                .Annotation("SqlServer:Include", new[] { "CorrectCount", "CorrectTotal", "CourseId", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "IsDeleted", "Percent", "SkillScoresStr", "Status", "StudentId", "SummaryNote", "UnitId", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" });

            migrationBuilder.CreateIndex(
                name: "IX_HomeWorkResults_StudentId",
                table: "HomeWorkResults",
                column: "StudentId")
                .Annotation("SqlServer:Include", new[] { "CorrectCount", "CorrectTotal", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "HomeWorkId", "IsDeleted", "LessonResultId", "Percent", "SkillScoresStr", "Status", "SubmissionCount", "TokenFirstTime", "TokenLastTime", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" });

            migrationBuilder.CreateIndex(
                name: "IX_CourseUnitMockTests_IsDeleted_CourseId",
                table: "CourseUnitMockTests",
                columns: new[] { "IsDeleted", "CourseId" });

            migrationBuilder.CreateIndex(
                name: "IX_CourseResults_IsDeleted_WorkingStatus",
                table: "CourseResults",
                columns: new[] { "IsDeleted", "WorkingStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_ClassForumResults_IsDeleted_Status_ClassForumId_Id",
                table: "ClassForumResults",
                columns: new[] { "IsDeleted", "Status", "ClassForumId", "Id" })
                .Annotation("SqlServer:Include", new[] { "StudentId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_VideoTimeCodeResults_VideoResultId_Status",
                table: "VideoTimeCodeResults");

            migrationBuilder.DropIndex(
                name: "IX_StudentFeedbacks_IsDeleted_ObjectId",
                table: "StudentFeedbacks");

            migrationBuilder.DropIndex(
                name: "IX_PlacementTestGroupResults_CreatedDate",
                table: "PlacementTestGroupResults");

            migrationBuilder.DropIndex(
                name: "IX_LessonResults_CourseId_StudentId_Status",
                table: "LessonResults");

            migrationBuilder.DropIndex(
                name: "IX_LessonResults_LessonId",
                table: "LessonResults");

            migrationBuilder.DropIndex(
                name: "IX_HomeWorkResults_StudentId",
                table: "HomeWorkResults");

            migrationBuilder.DropIndex(
                name: "IX_CourseUnitMockTests_IsDeleted_CourseId",
                table: "CourseUnitMockTests");

            migrationBuilder.DropIndex(
                name: "IX_CourseResults_IsDeleted_WorkingStatus",
                table: "CourseResults");

            migrationBuilder.DropIndex(
                name: "IX_ClassForumResults_IsDeleted_Status_ClassForumId_Id",
                table: "ClassForumResults");

            migrationBuilder.CreateIndex(
                name: "IX_LessonResults_LessonId",
                table: "LessonResults",
                column: "LessonId");

            migrationBuilder.CreateIndex(
                name: "IX_HomeWorkResults_StudentId",
                table: "HomeWorkResults",
                column: "StudentId");
        }
    }
}
