using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateConfigUnique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CourseClassStudents");

            migrationBuilder.DropIndex(
                name: "IX_VideoResults_LessonResultId",
                table: "VideoResults");

            migrationBuilder.DropIndex(
                name: "IX_CourseResults_CourseId",
                table: "CourseResults");

            migrationBuilder.DropIndex(
                name: "IX_ClassForums_LessonId",
                table: "ClassForums");

            migrationBuilder.CreateIndex(
                name: "IX_VideoResults_LessonResultId",
                table: "VideoResults",
                column: "LessonResultId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseResults_CourseId",
                table: "CourseResults",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassForums_LessonId",
                table: "ClassForums",
                column: "LessonId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_VideoResults_LessonResultId",
                table: "VideoResults");

            migrationBuilder.DropIndex(
                name: "IX_CourseResults_CourseId",
                table: "CourseResults");

            migrationBuilder.DropIndex(
                name: "IX_ClassForums_LessonId",
                table: "ClassForums");

            migrationBuilder.CreateTable(
                name: "CourseClassStudents",
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
                    CourseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClassId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseClassStudents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CourseClassStudents_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VideoResults_LessonResultId",
                table: "VideoResults",
                column: "LessonResultId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CourseResults_CourseId",
                table: "CourseResults",
                column: "CourseId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClassForums_LessonId",
                table: "ClassForums",
                column: "LessonId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CourseClassStudents_CourseId",
                table: "CourseClassStudents",
                column: "CourseId");
        }
    }
}
