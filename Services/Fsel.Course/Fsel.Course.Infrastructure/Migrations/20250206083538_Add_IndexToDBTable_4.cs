using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_IndexToDBTable_4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LessonResults_LessonId",
                table: "LessonResults");

            migrationBuilder.CreateIndex(
                name: "IX_PlacementTestGroupResults_IsDeleted_Status",
                table: "PlacementTestGroupResults",
                columns: new[] { "IsDeleted", "Status" })
                .Annotation("SqlServer:Include", new[] { "SuggetLevel", "StudentId" });

            migrationBuilder.CreateIndex(
                name: "IX_LessonResults_LessonId",
                table: "LessonResults",
                column: "LessonId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PlacementTestGroupResults_IsDeleted_Status",
                table: "PlacementTestGroupResults");

            migrationBuilder.DropIndex(
                name: "IX_LessonResults_LessonId",
                table: "LessonResults");

            migrationBuilder.CreateIndex(
                name: "IX_LessonResults_LessonId",
                table: "LessonResults",
                column: "LessonId")
                .Annotation("SqlServer:Include", new[] { "CorrectCount", "CorrectTotal", "CourseId", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "IsDeleted", "Percent", "SkillScoresStr", "Status", "StudentId", "SummaryNote", "UnitId", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" });
        }
    }
}
