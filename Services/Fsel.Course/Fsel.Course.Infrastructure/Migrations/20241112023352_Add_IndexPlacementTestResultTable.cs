using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_IndexPlacementTestResultTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_PlacementTestResults_Level_StudentId",
                table: "PlacementTestResults",
                columns: new[] { "Level", "StudentId" });

            migrationBuilder.CreateIndex(
                name: "IX_PlacementTestResults_Status_StudentId",
                table: "PlacementTestResults",
                columns: new[] { "Status", "StudentId" });

            migrationBuilder.CreateIndex(
                name: "IX_PlacementTestResults_StudentId",
                table: "PlacementTestResults",
                column: "StudentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PlacementTestResults_Level_StudentId",
                table: "PlacementTestResults");

            migrationBuilder.DropIndex(
                name: "IX_PlacementTestResults_Status_StudentId",
                table: "PlacementTestResults");

            migrationBuilder.DropIndex(
                name: "IX_PlacementTestResults_StudentId",
                table: "PlacementTestResults");
        }
    }
}
