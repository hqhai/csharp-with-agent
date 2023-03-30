using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUnitResult : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LessonResult_Courses_CourseId",
                table: "LessonResult");

            migrationBuilder.DropForeignKey(
                name: "FK_LessonResult_Lessons_LessonId",
                table: "LessonResult");

            migrationBuilder.DropForeignKey(
                name: "FK_LessonResult_UnitLessons_UnitLessonId",
                table: "LessonResult");

            migrationBuilder.DropForeignKey(
                name: "FK_LessonResult_Units_UnitId",
                table: "LessonResult");

            migrationBuilder.DropForeignKey(
                name: "FK_VideoResults_LessonResult_LessonResultId",
                table: "VideoResults");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LessonResult",
                table: "LessonResult");

            migrationBuilder.RenameTable(
                name: "LessonResult",
                newName: "LessonResults");

            migrationBuilder.RenameIndex(
                name: "IX_LessonResult_UnitLessonId",
                table: "LessonResults",
                newName: "IX_LessonResults_UnitLessonId");

            migrationBuilder.RenameIndex(
                name: "IX_LessonResult_UnitId",
                table: "LessonResults",
                newName: "IX_LessonResults_UnitId");

            migrationBuilder.RenameIndex(
                name: "IX_LessonResult_LessonId",
                table: "LessonResults",
                newName: "IX_LessonResults_LessonId");

            migrationBuilder.RenameIndex(
                name: "IX_LessonResult_CourseId",
                table: "LessonResults",
                newName: "IX_LessonResults_CourseId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LessonResults",
                table: "LessonResults",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "UnitResults",
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
                    Percent = table.Column<double>(type: "float", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CourseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CourseUnitMockTestId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnitResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UnitResults_CourseUnitMockTests_CourseUnitMockTestId",
                        column: x => x.CourseUnitMockTestId,
                        principalTable: "CourseUnitMockTests",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UnitResults_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UnitResults_Units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UnitResults_CourseId",
                table: "UnitResults",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_UnitResults_CourseUnitMockTestId",
                table: "UnitResults",
                column: "CourseUnitMockTestId");

            migrationBuilder.CreateIndex(
                name: "IX_UnitResults_UnitId",
                table: "UnitResults",
                column: "UnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_LessonResults_Courses_CourseId",
                table: "LessonResults",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LessonResults_Lessons_LessonId",
                table: "LessonResults",
                column: "LessonId",
                principalTable: "Lessons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LessonResults_UnitLessons_UnitLessonId",
                table: "LessonResults",
                column: "UnitLessonId",
                principalTable: "UnitLessons",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LessonResults_Units_UnitId",
                table: "LessonResults",
                column: "UnitId",
                principalTable: "Units",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VideoResults_LessonResults_LessonResultId",
                table: "VideoResults",
                column: "LessonResultId",
                principalTable: "LessonResults",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LessonResults_Courses_CourseId",
                table: "LessonResults");

            migrationBuilder.DropForeignKey(
                name: "FK_LessonResults_Lessons_LessonId",
                table: "LessonResults");

            migrationBuilder.DropForeignKey(
                name: "FK_LessonResults_UnitLessons_UnitLessonId",
                table: "LessonResults");

            migrationBuilder.DropForeignKey(
                name: "FK_LessonResults_Units_UnitId",
                table: "LessonResults");

            migrationBuilder.DropForeignKey(
                name: "FK_VideoResults_LessonResults_LessonResultId",
                table: "VideoResults");

            migrationBuilder.DropTable(
                name: "UnitResults");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LessonResults",
                table: "LessonResults");

            migrationBuilder.RenameTable(
                name: "LessonResults",
                newName: "LessonResult");

            migrationBuilder.RenameIndex(
                name: "IX_LessonResults_UnitLessonId",
                table: "LessonResult",
                newName: "IX_LessonResult_UnitLessonId");

            migrationBuilder.RenameIndex(
                name: "IX_LessonResults_UnitId",
                table: "LessonResult",
                newName: "IX_LessonResult_UnitId");

            migrationBuilder.RenameIndex(
                name: "IX_LessonResults_LessonId",
                table: "LessonResult",
                newName: "IX_LessonResult_LessonId");

            migrationBuilder.RenameIndex(
                name: "IX_LessonResults_CourseId",
                table: "LessonResult",
                newName: "IX_LessonResult_CourseId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LessonResult",
                table: "LessonResult",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LessonResult_Courses_CourseId",
                table: "LessonResult",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LessonResult_Lessons_LessonId",
                table: "LessonResult",
                column: "LessonId",
                principalTable: "Lessons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LessonResult_UnitLessons_UnitLessonId",
                table: "LessonResult",
                column: "UnitLessonId",
                principalTable: "UnitLessons",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LessonResult_Units_UnitId",
                table: "LessonResult",
                column: "UnitId",
                principalTable: "Units",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VideoResults_LessonResult_LessonResultId",
                table: "VideoResults",
                column: "LessonResultId",
                principalTable: "LessonResult",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
