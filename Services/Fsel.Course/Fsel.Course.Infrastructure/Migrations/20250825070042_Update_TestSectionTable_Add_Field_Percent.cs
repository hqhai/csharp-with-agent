using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_TestSectionTable_Add_Field_Percent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Percent",
                table: "TestSections",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ScoringFormulaConfigsStr",
                table: "TestSections",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ScoringFormulaType",
                table: "Tests",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Percent",
                table: "TestSections");

            migrationBuilder.DropColumn(
                name: "ScoringFormulaConfigsStr",
                table: "TestSections");

            migrationBuilder.DropColumn(
                name: "ScoringFormulaType",
                table: "Tests");
        }
    }
}
