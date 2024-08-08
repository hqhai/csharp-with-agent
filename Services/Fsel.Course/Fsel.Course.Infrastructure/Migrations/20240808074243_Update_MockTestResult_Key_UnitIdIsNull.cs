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
            migrationBuilder.CreateIndex(
                name: "IX_MockTestResults_CourseId_MockTestId_StudentId",
                table: "MockTestResults",
                columns: new[] { "CourseId", "MockTestId", "StudentId" },
                unique: true,
                filter: "[UnitId] IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MockTestResults_CourseId_MockTestId_StudentId",
                table: "MockTestResults");
        }
    }
}
