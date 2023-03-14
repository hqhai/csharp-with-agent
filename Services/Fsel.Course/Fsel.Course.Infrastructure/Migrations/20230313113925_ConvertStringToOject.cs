using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ConvertStringToOject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClassForums_Lessons_LessonId",
                table: "ClassForums");

            migrationBuilder.DropIndex(
                name: "IX_ClassForums_LessonId",
                table: "ClassForums");

            migrationBuilder.RenameColumn(
                name: "Config",
                table: "Questions",
                newName: "ConfigStr");

            migrationBuilder.AlterColumn<Guid>(
                name: "UnitId",
                table: "CourseUnitMockTests",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "MockTestId",
                table: "CourseUnitMockTests",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "LessonId",
                table: "ClassForums",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClassForums_LessonId",
                table: "ClassForums",
                column: "LessonId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ClassForums_Lessons_LessonId",
                table: "ClassForums",
                column: "LessonId",
                principalTable: "Lessons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClassForums_Lessons_LessonId",
                table: "ClassForums");

            migrationBuilder.DropIndex(
                name: "IX_ClassForums_LessonId",
                table: "ClassForums");

            migrationBuilder.RenameColumn(
                name: "ConfigStr",
                table: "Questions",
                newName: "Config");

            migrationBuilder.AlterColumn<Guid>(
                name: "UnitId",
                table: "CourseUnitMockTests",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "MockTestId",
                table: "CourseUnitMockTests",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "LessonId",
                table: "ClassForums",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.CreateIndex(
                name: "IX_ClassForums_LessonId",
                table: "ClassForums",
                column: "LessonId",
                unique: true,
                filter: "[LessonId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_ClassForums_Lessons_LessonId",
                table: "ClassForums",
                column: "LessonId",
                principalTable: "Lessons",
                principalColumn: "Id");
        }
    }
}
