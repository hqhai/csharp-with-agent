using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_PlacmentTest_By_Program : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Level",
                table: "PlacementTests",
                newName: "PlacementTestLevel");

            migrationBuilder.AddColumn<Guid>(
                name: "LevelId",
                table: "PlacementTests",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProgramId",
                table: "PlacementTests",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlacementTests_LevelId",
                table: "PlacementTests",
                column: "LevelId");

            migrationBuilder.CreateIndex(
                name: "IX_PlacementTests_ProgramId",
                table: "PlacementTests",
                column: "ProgramId");

            migrationBuilder.AddForeignKey(
                name: "FK_PlacementTests_Categorys_ProgramId",
                table: "PlacementTests",
                column: "ProgramId",
                principalTable: "Categorys",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PlacementTests_Levels_LevelId",
                table: "PlacementTests",
                column: "LevelId",
                principalTable: "Levels",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlacementTests_Categorys_ProgramId",
                table: "PlacementTests");

            migrationBuilder.DropForeignKey(
                name: "FK_PlacementTests_Levels_LevelId",
                table: "PlacementTests");

            migrationBuilder.DropIndex(
                name: "IX_PlacementTests_LevelId",
                table: "PlacementTests");

            migrationBuilder.DropIndex(
                name: "IX_PlacementTests_ProgramId",
                table: "PlacementTests");

            migrationBuilder.DropColumn(
                name: "LevelId",
                table: "PlacementTests");

            migrationBuilder.DropColumn(
                name: "ProgramId",
                table: "PlacementTests");

            migrationBuilder.RenameColumn(
                name: "PlacementTestLevel",
                table: "PlacementTests",
                newName: "Level");
        }
    }
}
