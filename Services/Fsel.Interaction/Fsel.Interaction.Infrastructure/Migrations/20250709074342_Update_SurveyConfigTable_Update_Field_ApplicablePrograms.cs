using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Interaction.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_SurveyConfigTable_Update_Field_ApplicablePrograms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApplicableProgram",
                table: "SurveyConfigs");

            migrationBuilder.AddColumn<string>(
                name: "ApplicableProgramStr",
                table: "SurveyConfigs",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApplicableProgramStr",
                table: "SurveyConfigs");

            migrationBuilder.AddColumn<string>(
                name: "ApplicableProgram",
                table: "SurveyConfigs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }
    }
}
