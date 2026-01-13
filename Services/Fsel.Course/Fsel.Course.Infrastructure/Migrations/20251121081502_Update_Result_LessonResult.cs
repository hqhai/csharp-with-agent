using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_Result_LessonResult : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LessonResults_UnitResults_UnitModuleId",
                table: "LessonResults");

            migrationBuilder.CreateIndex(
                name: "IX_LessonResults_UnitResultId",
                table: "LessonResults",
                column: "UnitResultId");

            migrationBuilder.AddForeignKey(
                name: "FK_LessonResults_UnitResults_UnitResultId",
                table: "LessonResults",
                column: "UnitResultId",
                principalTable: "UnitResults",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LessonResults_UnitResults_UnitResultId",
                table: "LessonResults");

            migrationBuilder.DropIndex(
                name: "IX_LessonResults_UnitResultId",
                table: "LessonResults");

            migrationBuilder.AddForeignKey(
                name: "FK_LessonResults_UnitResults_UnitModuleId",
                table: "LessonResults",
                column: "UnitModuleId",
                principalTable: "UnitResults",
                principalColumn: "Id");
        }
    }
}
