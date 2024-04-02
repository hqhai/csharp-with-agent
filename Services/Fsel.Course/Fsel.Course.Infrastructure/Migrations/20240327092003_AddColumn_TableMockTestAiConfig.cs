using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddColumn_TableMockTestAiConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MockTestAISettings_SectionId",
                table: "MockTestAISettings");

            migrationBuilder.AddColumn<string>(
                name: "Criteria",
                table: "MockTestAISettings",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_MockTestAISettings_SectionId",
                table: "MockTestAISettings",
                column: "SectionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MockTestAISettings_SectionId",
                table: "MockTestAISettings");

            migrationBuilder.DropColumn(
                name: "Criteria",
                table: "MockTestAISettings");

            migrationBuilder.CreateIndex(
                name: "IX_MockTestAISettings_SectionId",
                table: "MockTestAISettings",
                column: "SectionId",
                unique: true,
                filter: "[SectionId] IS NOT NULL");
        }
    }
}
