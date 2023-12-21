using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_PlacementTestResultToPlacmentTestId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PlacementTestId",
                table: "PlacementTestResults",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlacementTestResults_PlacementTestId",
                table: "PlacementTestResults",
                column: "PlacementTestId");

            migrationBuilder.AddForeignKey(
                name: "FK_PlacementTestResults_PlacementTests_PlacementTestId",
                table: "PlacementTestResults",
                column: "PlacementTestId",
                principalTable: "PlacementTests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
        }
    }
}
