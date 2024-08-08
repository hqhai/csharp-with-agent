using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_MockTestResult_Key_UnitIdIsNull : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MockTestResults_CourseId_MockTestId_StudentId_Type",
                table: "MockTestResults");

            migrationBuilder.CreateIndex(
                name: "IX_MockTestResults_CourseId_MockTestId_StudentId_Type",
                table: "MockTestResults",
                columns: new[] { "CourseId", "MockTestId", "StudentId", "Type" },
                unique: true,
                filter: "[UnitId] IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MockTestResults_CourseId_MockTestId_StudentId_Type",
                table: "MockTestResults");

            migrationBuilder.CreateIndex(
                name: "IX_MockTestResults_CourseId_MockTestId_StudentId_Type",
                table: "MockTestResults",
                columns: new[] { "CourseId", "MockTestId", "StudentId", "Type" },
                unique: true);
        }
    }
}
