using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_Result_HighestStreak : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "HighestStreak",
                table: "VideoTimeCodeResults",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HighestStreak",
                table: "VideoResults",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HighestStreak",
                table: "MockTestResults",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HighestStreak",
                table: "FinalTestResults",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HighestStreak",
                table: "VideoTimeCodeResults");

            migrationBuilder.DropColumn(
                name: "HighestStreak",
                table: "VideoResults");

            migrationBuilder.DropColumn(
                name: "HighestStreak",
                table: "MockTestResults");

            migrationBuilder.DropColumn(
                name: "HighestStreak",
                table: "FinalTestResults");
        }
    }
}
