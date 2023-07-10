using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class update_ExtraPractice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "MockTestId",
                table: "ExtraPractices",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PlacementTestId",
                table: "ExtraPractices",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExtraPractices_MockTestId",
                table: "ExtraPractices",
                column: "MockTestId");

            migrationBuilder.CreateIndex(
                name: "IX_ExtraPractices_PlacementTestId",
                table: "ExtraPractices",
                column: "PlacementTestId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExtraPractices_MockTests_MockTestId",
                table: "ExtraPractices",
                column: "MockTestId",
                principalTable: "MockTests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ExtraPractices_PlacementTests_PlacementTestId",
                table: "ExtraPractices",
                column: "PlacementTestId",
                principalTable: "PlacementTests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExtraPractices_MockTests_MockTestId",
                table: "ExtraPractices");

            migrationBuilder.DropForeignKey(
                name: "FK_ExtraPractices_PlacementTests_PlacementTestId",
                table: "ExtraPractices");

            migrationBuilder.DropIndex(
                name: "IX_ExtraPractices_MockTestId",
                table: "ExtraPractices");

            migrationBuilder.DropIndex(
                name: "IX_ExtraPractices_PlacementTestId",
                table: "ExtraPractices");

            migrationBuilder.DropColumn(
                name: "MockTestId",
                table: "ExtraPractices");

            migrationBuilder.DropColumn(
                name: "PlacementTestId",
                table: "ExtraPractices");
        }
    }
}
