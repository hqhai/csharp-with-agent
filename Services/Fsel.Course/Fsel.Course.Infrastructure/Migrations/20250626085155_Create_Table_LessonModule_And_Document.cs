using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Create_Table_LessonModule_And_Document : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClassForums_Lessons_LessonId",
                table: "ClassForums");

            migrationBuilder.AlterColumn<string>(
                name: "InstructionContent",
                table: "Lessons",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ClassForumCount",
                table: "Lessons",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DocumentCount",
                table: "Lessons",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "HomeWorkCount",
                table: "Lessons",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "LevelId",
                table: "Lessons",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProgramId",
                table: "Lessons",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Lessons",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "VideoCount",
                table: "Lessons",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "PromptName",
                table: "ClassForums",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 250,
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "LessonId",
                table: "ClassForums",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<string>(
                name: "Layout",
                table: "ClassForums",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "ProgramId",
                table: "ClassForums",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Documents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedFullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UpdatedFullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DeletedFullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    FilesStr = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LessonModules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedFullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UpdatedFullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DeletedFullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Thumbnail = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    LessonConfigType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    DisplayNumber = table.Column<int>(type: "int", nullable: false),
                    Percent = table.Column<double>(type: "float", nullable: false),
                    OpenOrder = table.Column<int>(type: "int", nullable: false),
                    LessonId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    VideoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ClassForumId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    HomeWorkId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DocumentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LessonModules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LessonModules_ClassForums_ClassForumId",
                        column: x => x.ClassForumId,
                        principalTable: "ClassForums",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LessonModules_Documents_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LessonModules_HomeWorks_HomeWorkId",
                        column: x => x.HomeWorkId,
                        principalTable: "HomeWorks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LessonModules_Lessons_LessonId",
                        column: x => x.LessonId,
                        principalTable: "Lessons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LessonModules_Videos_VideoId",
                        column: x => x.VideoId,
                        principalTable: "Videos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Lessons_LevelId",
                table: "Lessons",
                column: "LevelId");

            migrationBuilder.CreateIndex(
                name: "IX_Lessons_ProgramId",
                table: "Lessons",
                column: "ProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassForums_ProgramId",
                table: "ClassForums",
                column: "ProgramId");

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
                name: "IX_LessonModules_LessonId",
                table: "LessonModules",
                column: "LessonId");

            migrationBuilder.CreateIndex(
                name: "IX_LessonModules_VideoId",
                table: "LessonModules",
                column: "VideoId");

            migrationBuilder.AddForeignKey(
                name: "FK_ClassForums_Categorys_ProgramId",
                table: "ClassForums",
                column: "ProgramId",
                principalTable: "Categorys",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ClassForums_Lessons_LessonId",
                table: "ClassForums",
                column: "LessonId",
                principalTable: "Lessons",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Lessons_Categorys_ProgramId",
                table: "Lessons",
                column: "ProgramId",
                principalTable: "Categorys",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Lessons_Levels_LevelId",
                table: "Lessons",
                column: "LevelId",
                principalTable: "Levels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClassForums_Categorys_ProgramId",
                table: "ClassForums");

            migrationBuilder.DropForeignKey(
                name: "FK_ClassForums_Lessons_LessonId",
                table: "ClassForums");

            migrationBuilder.DropForeignKey(
                name: "FK_Lessons_Categorys_ProgramId",
                table: "Lessons");

            migrationBuilder.DropForeignKey(
                name: "FK_Lessons_Levels_LevelId",
                table: "Lessons");

            migrationBuilder.DropTable(
                name: "LessonModules");

            migrationBuilder.DropTable(
                name: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_Lessons_LevelId",
                table: "Lessons");

            migrationBuilder.DropIndex(
                name: "IX_Lessons_ProgramId",
                table: "Lessons");

            migrationBuilder.DropIndex(
                name: "IX_ClassForums_ProgramId",
                table: "ClassForums");

            migrationBuilder.DropColumn(
                name: "ClassForumCount",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "DocumentCount",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "HomeWorkCount",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "LevelId",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "ProgramId",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "VideoCount",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "Layout",
                table: "ClassForums");

            migrationBuilder.DropColumn(
                name: "ProgramId",
                table: "ClassForums");

            migrationBuilder.AlterColumn<string>(
                name: "InstructionContent",
                table: "Lessons",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PromptName",
                table: "ClassForums",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(300)",
                oldMaxLength: 300,
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "LessonId",
                table: "ClassForums",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ClassForums_Lessons_LessonId",
                table: "ClassForums",
                column: "LessonId",
                principalTable: "Lessons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
