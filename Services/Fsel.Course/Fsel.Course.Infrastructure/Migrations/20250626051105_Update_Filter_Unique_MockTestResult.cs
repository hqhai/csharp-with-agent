using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_Filter_Unique_MockTestResult : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MockTestResults_CourseId_MockTestId_StudentId",
                table: "MockTestResults");

            migrationBuilder.DropIndex(
                name: "IX_MockTestResults_CourseId_MockTestId_UnitId_StudentId",
                table: "MockTestResults");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MockTestResults_CourseId_MockTestId_StudentId",
                table: "MockTestResults");

            migrationBuilder.DropIndex(
                name: "IX_MockTestResults_CourseId_MockTestId_UnitId_StudentId",
                table: "MockTestResults");

            migrationBuilder.CreateIndex(
                name: "IX_MockTestResults_CourseId_MockTestId_StudentId",
                table: "MockTestResults",
                columns: new[] { "CourseId", "MockTestId", "StudentId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_MockTestResults_CourseId_MockTestId_UnitId_StudentId",
                table: "MockTestResults",
                columns: new[] { "CourseId", "MockTestId", "UnitId", "StudentId" },
                unique: true,
                filter: "[UnitId] IS NOT NULL");
        }
    }
}
