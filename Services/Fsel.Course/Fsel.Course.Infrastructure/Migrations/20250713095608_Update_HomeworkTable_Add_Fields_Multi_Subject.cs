using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_HomeworkTable_Add_Fields_Multi_Subject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "HomeWorks",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 250);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "HomeWorks",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 250);

            migrationBuilder.AddColumn<Guid>(
                name: "LevelId",
                table: "HomeWorks",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OriginalId",
                table: "HomeWorks",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ProgramId",
                table: "HomeWorks",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "HomeWorks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "VersionStatus",
                table: "HomeWorks",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_HomeWorks_LevelId",
                table: "HomeWorks",
                column: "LevelId");

            migrationBuilder.CreateIndex(
                name: "IX_HomeWorks_ProgramId",
                table: "HomeWorks",
                column: "ProgramId");

            migrationBuilder.AddForeignKey(
                name: "FK_HomeWorks_Categorys_ProgramId",
                table: "HomeWorks",
                column: "ProgramId",
                principalTable: "Categorys",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_HomeWorks_Levels_LevelId",
                table: "HomeWorks",
                column: "LevelId",
                principalTable: "Levels",
                principalColumn: "Id");

            migrationBuilder.Sql(
                @"UPDATE HomeWorks
                SET OriginalId = NEWID()
                WHERE OriginalId = '00000000-0000-0000-0000-000000000000'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HomeWorks_Categorys_ProgramId",
                table: "HomeWorks");

            migrationBuilder.DropForeignKey(
                name: "FK_HomeWorks_Levels_LevelId",
                table: "HomeWorks");

            migrationBuilder.DropIndex(
                name: "IX_HomeWorks_LevelId",
                table: "HomeWorks");

            migrationBuilder.DropIndex(
                name: "IX_HomeWorks_ProgramId",
                table: "HomeWorks");

            migrationBuilder.DropColumn(
                name: "LevelId",
                table: "HomeWorks");

            migrationBuilder.DropColumn(
                name: "OriginalId",
                table: "HomeWorks");

            migrationBuilder.DropColumn(
                name: "ProgramId",
                table: "HomeWorks");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "HomeWorks");

            migrationBuilder.DropColumn(
                name: "VersionStatus",
                table: "HomeWorks");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "HomeWorks",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "HomeWorks",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);
        }
    }
}
