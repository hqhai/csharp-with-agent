using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_Answer_IsCorrect : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsCorrect",
                table: "VideoTimeCodeAnswers",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCorrect",
                table: "PlacementTestAnswers",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCorrect",
                table: "MockTestAnswers",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCorrect",
                table: "HomeWorkAnswers",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCorrect",
                table: "FinalTestAnswers",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCorrect",
                table: "ExtraPracticeAnswers",
                type: "bit",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCorrect",
                table: "VideoTimeCodeAnswers");

            migrationBuilder.DropColumn(
                name: "IsCorrect",
                table: "PlacementTestAnswers");

            migrationBuilder.DropColumn(
                name: "IsCorrect",
                table: "MockTestAnswers");

            migrationBuilder.DropColumn(
                name: "IsCorrect",
                table: "HomeWorkAnswers");

            migrationBuilder.DropColumn(
                name: "IsCorrect",
                table: "FinalTestAnswers");

            migrationBuilder.DropColumn(
                name: "IsCorrect",
                table: "ExtraPracticeAnswers");
        }
    }
}
