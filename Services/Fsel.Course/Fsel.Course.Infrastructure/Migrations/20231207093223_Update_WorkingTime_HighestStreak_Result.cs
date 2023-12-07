using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_WorkingTime_HighestStreak_Result : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "HighestStreak",
                table: "VideoTimeCodeResults",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFirstSubmit",
                table: "VideoTimeCodeAnswers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "HighestStreak",
                table: "VideoResults",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HighestStreak",
                table: "SectionGroupResults",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "WorkingTime",
                table: "SectionGroupResults",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "HighestStreak",
                table: "MockTestResults",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "WorkingTime",
                table: "MockTestResults",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "HighestStreak",
                table: "FinalTestResults",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "WorkingTime",
                table: "FinalTestResults",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HighestStreak",
                table: "VideoTimeCodeResults");

            migrationBuilder.DropColumn(
                name: "IsFirstSubmit",
                table: "VideoTimeCodeAnswers");

            migrationBuilder.DropColumn(
                name: "HighestStreak",
                table: "VideoResults");

            migrationBuilder.DropColumn(
                name: "HighestStreak",
                table: "SectionGroupResults");

            migrationBuilder.DropColumn(
                name: "WorkingTime",
                table: "SectionGroupResults");

            migrationBuilder.DropColumn(
                name: "HighestStreak",
                table: "MockTestResults");

            migrationBuilder.DropColumn(
                name: "WorkingTime",
                table: "MockTestResults");

            migrationBuilder.DropColumn(
                name: "HighestStreak",
                table: "FinalTestResults");

            migrationBuilder.DropColumn(
                name: "WorkingTime",
                table: "FinalTestResults");
        }
    }
}
