using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addHomeWorkClassForumExtra : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClassForum_Lessons_LessonId",
                table: "ClassForum");

            migrationBuilder.DropForeignKey(
                name: "FK_LessonExtraPractices_ExtraPractice_ExtracPraticeId",
                table: "LessonExtraPractices");

            migrationBuilder.DropForeignKey(
                name: "FK_LessonHomeWorks_HomeWork_HomeWorkId",
                table: "LessonHomeWorks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_HomeWork",
                table: "HomeWork");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ExtraPractice",
                table: "ExtraPractice");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ClassForum",
                table: "ClassForum");

            migrationBuilder.RenameTable(
                name: "HomeWork",
                newName: "HomeWorks");

            migrationBuilder.RenameTable(
                name: "ExtraPractice",
                newName: "ExtraPractices");

            migrationBuilder.RenameTable(
                name: "ClassForum",
                newName: "ClassForums");

            migrationBuilder.RenameIndex(
                name: "IX_ClassForum_LessonId",
                table: "ClassForums",
                newName: "IX_ClassForums_LessonId");

            migrationBuilder.AlterColumn<string>(
                name: "CourseSkill",
                table: "HomeWorks",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "CourseLevel",
                table: "HomeWorks",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "ExtraPractices",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "CourseLevel",
                table: "ExtraPractices",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "GradingStyle",
                table: "ClassForums",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "CourseSkill",
                table: "ClassForums",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddPrimaryKey(
                name: "PK_HomeWorks",
                table: "HomeWorks",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ExtraPractices",
                table: "ExtraPractices",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ClassForums",
                table: "ClassForums",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ClassForums_Lessons_LessonId",
                table: "ClassForums",
                column: "LessonId",
                principalTable: "Lessons",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LessonExtraPractices_ExtraPractices_ExtracPraticeId",
                table: "LessonExtraPractices",
                column: "ExtracPraticeId",
                principalTable: "ExtraPractices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LessonHomeWorks_HomeWorks_HomeWorkId",
                table: "LessonHomeWorks",
                column: "HomeWorkId",
                principalTable: "HomeWorks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClassForums_Lessons_LessonId",
                table: "ClassForums");

            migrationBuilder.DropForeignKey(
                name: "FK_LessonExtraPractices_ExtraPractices_ExtracPraticeId",
                table: "LessonExtraPractices");

            migrationBuilder.DropForeignKey(
                name: "FK_LessonHomeWorks_HomeWorks_HomeWorkId",
                table: "LessonHomeWorks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_HomeWorks",
                table: "HomeWorks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ExtraPractices",
                table: "ExtraPractices");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ClassForums",
                table: "ClassForums");

            migrationBuilder.RenameTable(
                name: "HomeWorks",
                newName: "HomeWork");

            migrationBuilder.RenameTable(
                name: "ExtraPractices",
                newName: "ExtraPractice");

            migrationBuilder.RenameTable(
                name: "ClassForums",
                newName: "ClassForum");

            migrationBuilder.RenameIndex(
                name: "IX_ClassForums_LessonId",
                table: "ClassForum",
                newName: "IX_ClassForum_LessonId");

            migrationBuilder.AlterColumn<int>(
                name: "CourseSkill",
                table: "HomeWork",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<int>(
                name: "CourseLevel",
                table: "HomeWork",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<int>(
                name: "Type",
                table: "ExtraPractice",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<int>(
                name: "CourseLevel",
                table: "ExtraPractice",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<int>(
                name: "GradingStyle",
                table: "ClassForum",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<int>(
                name: "CourseSkill",
                table: "ClassForum",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddPrimaryKey(
                name: "PK_HomeWork",
                table: "HomeWork",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ExtraPractice",
                table: "ExtraPractice",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ClassForum",
                table: "ClassForum",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ClassForum_Lessons_LessonId",
                table: "ClassForum",
                column: "LessonId",
                principalTable: "Lessons",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LessonExtraPractices_ExtraPractice_ExtracPraticeId",
                table: "LessonExtraPractices",
                column: "ExtracPraticeId",
                principalTable: "ExtraPractice",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LessonHomeWorks_HomeWork_HomeWorkId",
                table: "LessonHomeWorks",
                column: "HomeWorkId",
                principalTable: "HomeWork",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
