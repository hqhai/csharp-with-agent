using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_Video_Program : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Videos",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 250);

            migrationBuilder.AddColumn<Guid>(
                name: "LevelId",
                table: "Videos",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OriginalId",
                table: "Videos",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProgramId",
                table: "Videos",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "Videos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "VersionStatus",
                table: "Videos",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Videos_LevelId",
                table: "Videos",
                column: "LevelId");

            migrationBuilder.CreateIndex(
                name: "IX_Videos_OriginalId",
                table: "Videos",
                column: "OriginalId");

            migrationBuilder.CreateIndex(
                name: "IX_Videos_ProgramId",
                table: "Videos",
                column: "ProgramId");

            migrationBuilder.AddForeignKey(
                name: "FK_Videos_Categorys_ProgramId",
                table: "Videos",
                column: "ProgramId",
                principalTable: "Categorys",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Videos_Levels_LevelId",
                table: "Videos",
                column: "LevelId",
                principalTable: "Levels",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Videos_Videos_OriginalId",
                table: "Videos",
                column: "OriginalId",
                principalTable: "Videos",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Videos_Categorys_ProgramId",
                table: "Videos");

            migrationBuilder.DropForeignKey(
                name: "FK_Videos_Levels_LevelId",
                table: "Videos");

            migrationBuilder.DropForeignKey(
                name: "FK_Videos_Videos_OriginalId",
                table: "Videos");

            migrationBuilder.DropIndex(
                name: "IX_Videos_LevelId",
                table: "Videos");

            migrationBuilder.DropIndex(
                name: "IX_Videos_OriginalId",
                table: "Videos");

            migrationBuilder.DropIndex(
                name: "IX_Videos_ProgramId",
                table: "Videos");

            migrationBuilder.DropColumn(
                name: "LevelId",
                table: "Videos");

            migrationBuilder.DropColumn(
                name: "OriginalId",
                table: "Videos");

            migrationBuilder.DropColumn(
                name: "ProgramId",
                table: "Videos");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "Videos");

            migrationBuilder.DropColumn(
                name: "VersionStatus",
                table: "Videos");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Videos",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);
        }
    }
}
