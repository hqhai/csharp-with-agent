using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_UnitResult_LessonResult : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CourseUnitMockTestResults_CourseUnitMockTests_CourseUnitMockTestId",
                table: "CourseUnitMockTestResults");

            migrationBuilder.DropForeignKey(
                name: "FK_UnitLessonResults_UnitLessons_UnitLessonId",
                table: "UnitLessonResults");

            migrationBuilder.AlterColumn<Guid>(
                name: "UnitLessonId",
                table: "UnitLessonResults",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<Guid>(
                name: "CourseId",
                table: "UnitLessonResults",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "LessonId",
                table: "UnitLessonResults",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "UnitId",
                table: "UnitLessonResults",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "CourseUnitMockTestResults",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<Guid>(
                name: "CourseUnitMockTestId",
                table: "CourseUnitMockTestResults",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<Guid>(
                name: "CourseId",
                table: "CourseUnitMockTestResults",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "UnitId",
                table: "CourseUnitMockTestResults",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_UnitLessonResults_CourseId",
                table: "UnitLessonResults",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_UnitLessonResults_LessonId",
                table: "UnitLessonResults",
                column: "LessonId");

            migrationBuilder.CreateIndex(
                name: "IX_UnitLessonResults_UnitId",
                table: "UnitLessonResults",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseUnitMockTestResults_CourseId",
                table: "CourseUnitMockTestResults",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseUnitMockTestResults_UnitId",
                table: "CourseUnitMockTestResults",
                column: "UnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_CourseUnitMockTestResults_CourseUnitMockTests_CourseUnitMockTestId",
                table: "CourseUnitMockTestResults",
                column: "CourseUnitMockTestId",
                principalTable: "CourseUnitMockTests",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CourseUnitMockTestResults_Courses_CourseId",
                table: "CourseUnitMockTestResults",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CourseUnitMockTestResults_Units_UnitId",
                table: "CourseUnitMockTestResults",
                column: "UnitId",
                principalTable: "Units",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UnitLessonResults_Courses_CourseId",
                table: "UnitLessonResults",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UnitLessonResults_Lessons_LessonId",
                table: "UnitLessonResults",
                column: "LessonId",
                principalTable: "Lessons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UnitLessonResults_UnitLessons_UnitLessonId",
                table: "UnitLessonResults",
                column: "UnitLessonId",
                principalTable: "UnitLessons",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UnitLessonResults_Units_UnitId",
                table: "UnitLessonResults",
                column: "UnitId",
                principalTable: "Units",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CourseUnitMockTestResults_CourseUnitMockTests_CourseUnitMockTestId",
                table: "CourseUnitMockTestResults");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseUnitMockTestResults_Courses_CourseId",
                table: "CourseUnitMockTestResults");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseUnitMockTestResults_Units_UnitId",
                table: "CourseUnitMockTestResults");

            migrationBuilder.DropForeignKey(
                name: "FK_UnitLessonResults_Courses_CourseId",
                table: "UnitLessonResults");

            migrationBuilder.DropForeignKey(
                name: "FK_UnitLessonResults_Lessons_LessonId",
                table: "UnitLessonResults");

            migrationBuilder.DropForeignKey(
                name: "FK_UnitLessonResults_UnitLessons_UnitLessonId",
                table: "UnitLessonResults");

            migrationBuilder.DropForeignKey(
                name: "FK_UnitLessonResults_Units_UnitId",
                table: "UnitLessonResults");

            migrationBuilder.DropIndex(
                name: "IX_UnitLessonResults_CourseId",
                table: "UnitLessonResults");

            migrationBuilder.DropIndex(
                name: "IX_UnitLessonResults_LessonId",
                table: "UnitLessonResults");

            migrationBuilder.DropIndex(
                name: "IX_UnitLessonResults_UnitId",
                table: "UnitLessonResults");

            migrationBuilder.DropIndex(
                name: "IX_CourseUnitMockTestResults_CourseId",
                table: "CourseUnitMockTestResults");

            migrationBuilder.DropIndex(
                name: "IX_CourseUnitMockTestResults_UnitId",
                table: "CourseUnitMockTestResults");

            migrationBuilder.DropColumn(
                name: "CourseId",
                table: "UnitLessonResults");

            migrationBuilder.DropColumn(
                name: "LessonId",
                table: "UnitLessonResults");

            migrationBuilder.DropColumn(
                name: "UnitId",
                table: "UnitLessonResults");

            migrationBuilder.DropColumn(
                name: "CourseId",
                table: "CourseUnitMockTestResults");

            migrationBuilder.DropColumn(
                name: "UnitId",
                table: "CourseUnitMockTestResults");

            migrationBuilder.AlterColumn<Guid>(
                name: "UnitLessonId",
                table: "UnitLessonResults",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "CourseUnitMockTestResults",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<Guid>(
                name: "CourseUnitMockTestId",
                table: "CourseUnitMockTestResults",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CourseUnitMockTestResults_CourseUnitMockTests_CourseUnitMockTestId",
                table: "CourseUnitMockTestResults",
                column: "CourseUnitMockTestId",
                principalTable: "CourseUnitMockTests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UnitLessonResults_UnitLessons_UnitLessonId",
                table: "UnitLessonResults",
                column: "UnitLessonId",
                principalTable: "UnitLessons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
