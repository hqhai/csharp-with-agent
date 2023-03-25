using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class fixTableCourseClasses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CourseClass_Courses_CourseId",
                table: "CourseClass");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CourseClass",
                table: "CourseClass");

            migrationBuilder.RenameTable(
                name: "CourseClass",
                newName: "CourseClasses");

            migrationBuilder.RenameIndex(
                name: "IX_CourseClass_CourseId",
                table: "CourseClasses",
                newName: "IX_CourseClasses_CourseId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CourseClasses",
                table: "CourseClasses",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CourseClasses_Courses_CourseId",
                table: "CourseClasses",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CourseClasses_Courses_CourseId",
                table: "CourseClasses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CourseClasses",
                table: "CourseClasses");

            migrationBuilder.RenameTable(
                name: "CourseClasses",
                newName: "CourseClass");

            migrationBuilder.RenameIndex(
                name: "IX_CourseClasses_CourseId",
                table: "CourseClass",
                newName: "IX_CourseClass_CourseId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CourseClass",
                table: "CourseClass",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CourseClass_Courses_CourseId",
                table: "CourseClass",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
