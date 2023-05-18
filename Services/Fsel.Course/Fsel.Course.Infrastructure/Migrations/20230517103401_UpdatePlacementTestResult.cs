using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePlacementTestResult : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlacementTestResults_PlacementTests_PlacementTestId",
                table: "PlacementTestResults");

            migrationBuilder.DropIndex(
                name: "IX_PlacementTestResults_PlacementTestId",
                table: "PlacementTestResults");

            migrationBuilder.DropColumn(
                name: "PlacementTestId",
                table: "PlacementTestResults");

            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "Sections",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SkillScoresStr",
                table: "PlacementTestResults",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CorrectCount",
                table: "PlacementTestAnswers",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "Sections");

            migrationBuilder.DropColumn(
                name: "SkillScoresStr",
                table: "PlacementTestResults");

            migrationBuilder.DropColumn(
                name: "CorrectCount",
                table: "PlacementTestAnswers");

            migrationBuilder.AddColumn<Guid>(
                name: "PlacementTestId",
                table: "PlacementTestResults",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_PlacementTestResults_PlacementTestId",
                table: "PlacementTestResults",
                column: "PlacementTestId");

            migrationBuilder.AddForeignKey(
                name: "FK_PlacementTestResults_PlacementTests_PlacementTestId",
                table: "PlacementTestResults",
                column: "PlacementTestId",
                principalTable: "PlacementTests",
                principalColumn: "Id");
        }
    }
}
