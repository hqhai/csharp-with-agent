using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_Lesson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LessonModules_ClassForums_ClassForumId",
                table: "LessonModules");

            migrationBuilder.DropForeignKey(
                name: "FK_LessonModules_Documents_DocumentId",
                table: "LessonModules");

            migrationBuilder.DropForeignKey(
                name: "FK_LessonModules_HomeWorks_HomeWorkId",
                table: "LessonModules");

            migrationBuilder.DropForeignKey(
                name: "FK_LessonModules_Videos_VideoId",
                table: "LessonModules");

            migrationBuilder.DropIndex(
                name: "IX_LessonModules_ClassForumId",
                table: "LessonModules");

            migrationBuilder.DropIndex(
                name: "IX_LessonModules_DocumentId",
                table: "LessonModules");

            migrationBuilder.DropIndex(
                name: "IX_LessonModules_HomeWorkId",
                table: "LessonModules");

            migrationBuilder.DropIndex(
                name: "IX_LessonModules_VideoId",
                table: "LessonModules");

            migrationBuilder.DropColumn(
                name: "ClassForumId",
                table: "LessonModules");

            migrationBuilder.DropColumn(
                name: "DocumentId",
                table: "LessonModules");

            migrationBuilder.DropColumn(
                name: "HomeWorkId",
                table: "LessonModules");

            migrationBuilder.DropColumn(
                name: "VideoId",
                table: "LessonModules");

            migrationBuilder.AddColumn<Guid>(
                name: "OriginalId",
                table: "Lessons",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "Lessons",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "VersionStatus",
                table: "Lessons",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "LessonModules",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AddColumn<Guid>(
                name: "OriginalId",
                table: "LessonModules",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "OriginalId",
                table: "Documents",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "Documents",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "VersionStatus",
                table: "Documents",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "OriginalId",
                table: "ClassForums",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "ClassForums",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "VersionStatus",
                table: "ClassForums",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OriginalId",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "VersionStatus",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "OriginalId",
                table: "LessonModules");

            migrationBuilder.DropColumn(
                name: "OriginalId",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "VersionStatus",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "OriginalId",
                table: "ClassForums");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "ClassForums");

            migrationBuilder.DropColumn(
                name: "VersionStatus",
                table: "ClassForums");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "LessonModules",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ClassForumId",
                table: "LessonModules",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DocumentId",
                table: "LessonModules",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "HomeWorkId",
                table: "LessonModules",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "VideoId",
                table: "LessonModules",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LessonModules_ClassForumId",
                table: "LessonModules",
                column: "ClassForumId");

            migrationBuilder.CreateIndex(
                name: "IX_LessonModules_DocumentId",
                table: "LessonModules",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_LessonModules_HomeWorkId",
                table: "LessonModules",
                column: "HomeWorkId");

            migrationBuilder.CreateIndex(
                name: "IX_LessonModules_VideoId",
                table: "LessonModules",
                column: "VideoId");

            migrationBuilder.AddForeignKey(
                name: "FK_LessonModules_ClassForums_ClassForumId",
                table: "LessonModules",
                column: "ClassForumId",
                principalTable: "ClassForums",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LessonModules_Documents_DocumentId",
                table: "LessonModules",
                column: "DocumentId",
                principalTable: "Documents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LessonModules_HomeWorks_HomeWorkId",
                table: "LessonModules",
                column: "HomeWorkId",
                principalTable: "HomeWorks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LessonModules_Videos_VideoId",
                table: "LessonModules",
                column: "VideoId",
                principalTable: "Videos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
