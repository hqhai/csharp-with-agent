using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddChangeCourseHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CourseChangingHistories",
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
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ToLevelId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SelectedLevelId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ToProgramId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SelectedProgramId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ToCourseResultId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FromInfoStr = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    PtResultId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Action = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseChangingHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CourseChangingHistories_CourseResults_ToCourseResultId",
                        column: x => x.ToCourseResultId,
                        principalTable: "CourseResults",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CourseChangingHistories_TestGroupResult_PtResultId",
                        column: x => x.PtResultId,
                        principalTable: "TestGroupResult",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CourseChangingHistories_PtResultId",
                table: "CourseChangingHistories",
                column: "PtResultId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseChangingHistories_ToCourseResultId",
                table: "CourseChangingHistories",
                column: "ToCourseResultId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CourseChangingHistories");
        }
    }
}
