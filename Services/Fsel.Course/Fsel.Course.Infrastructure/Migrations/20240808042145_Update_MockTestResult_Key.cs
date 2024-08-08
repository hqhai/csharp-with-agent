using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_MockTestResult_Key : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MockTestResults_CourseId_MockTestId_UnitId_StudentId",
                table: "MockTestResults");

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "MockTestResults",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                computedColumnSql: "IIF(UnitId IS NULL, 'FullMockTest', 'SkillMockTest')");

            migrationBuilder.CreateIndex(
                name: "IX_MockTestResults_CourseId_MockTestId_StudentId_Type",
                table: "MockTestResults",
                columns: new[] { "CourseId", "MockTestId", "StudentId", "Type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MockTestResults_CourseId_MockTestId_UnitId_StudentId_Type",
                table: "MockTestResults",
                columns: new[] { "CourseId", "MockTestId", "UnitId", "StudentId", "Type" },
                unique: true,
                filter: "[UnitId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MockTestResults_CourseId_MockTestId_StudentId_Type",
                table: "MockTestResults");

            migrationBuilder.DropIndex(
                name: "IX_MockTestResults_CourseId_MockTestId_UnitId_StudentId_Type",
                table: "MockTestResults");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "MockTestResults");

            migrationBuilder.CreateIndex(
                name: "IX_MockTestResults_CourseId_MockTestId_UnitId_StudentId",
                table: "MockTestResults",
                columns: new[] { "CourseId", "MockTestId", "UnitId", "StudentId" },
                unique: true,
                filter: "[UnitId] IS NOT NULL");
        }
    }
}
