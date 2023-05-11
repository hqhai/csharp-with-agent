using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_MockTestSectionTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MockTestSections_MockTests_SectionGroupId",
                table: "MockTestSections");

            migrationBuilder.CreateIndex(
                name: "IX_MockTestSections_MockTestId",
                table: "MockTestSections",
                column: "MockTestId");

            migrationBuilder.AddForeignKey(
                name: "FK_MockTestSections_MockTests_MockTestId",
                table: "MockTestSections",
                column: "MockTestId",
                principalTable: "MockTests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MockTestSections_MockTests_MockTestId",
                table: "MockTestSections");

            migrationBuilder.DropIndex(
                name: "IX_MockTestSections_MockTestId",
                table: "MockTestSections");

            migrationBuilder.AddForeignKey(
                name: "FK_MockTestSections_MockTests_SectionGroupId",
                table: "MockTestSections",
                column: "SectionGroupId",
                principalTable: "MockTests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
