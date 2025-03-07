using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_UnitResult_IsUnique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UnitResults_CourseId",
                table: "UnitResults");

            migrationBuilder.CreateIndex(
                name: "IX_UnitResults_CourseId_UnitId_StudentId",
                table: "UnitResults",
                columns: new[] { "CourseId", "UnitId", "StudentId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UnitResults_CourseId_UnitId_StudentId",
                table: "UnitResults");

            migrationBuilder.CreateIndex(
                name: "IX_UnitResults_CourseId",
                table: "UnitResults",
                column: "CourseId");
        }
    }
}
