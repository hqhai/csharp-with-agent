using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_TestGroupResult_EmailLevelId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "EmailLevelId",
                table: "TestGroupResult",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TestGroupResult_EmailLevelId",
                table: "TestGroupResult",
                column: "EmailLevelId");

            migrationBuilder.AddForeignKey(
                name: "FK_TestGroupResult_Levels_EmailLevelId",
                table: "TestGroupResult",
                column: "EmailLevelId",
                principalTable: "Levels",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TestGroupResult_Levels_EmailLevelId",
                table: "TestGroupResult");

            migrationBuilder.DropIndex(
                name: "IX_TestGroupResult_EmailLevelId",
                table: "TestGroupResult");

            migrationBuilder.DropColumn(
                name: "EmailLevelId",
                table: "TestGroupResult");
        }
    }
}
