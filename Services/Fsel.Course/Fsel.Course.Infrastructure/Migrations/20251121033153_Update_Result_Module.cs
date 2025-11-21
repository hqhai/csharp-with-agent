using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_Result_Module : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UnitResults_CreatedUserId",
                table: "UnitResults");

            migrationBuilder.DropIndex(
                name: "IX_LessonResults_CreatedUserId",
                table: "LessonResults");

            migrationBuilder.DropIndex(
                name: "IX_HomeWorkResults_StudentId",
                table: "HomeWorkResults");

            migrationBuilder.AddColumn<Guid>(
                name: "LessonModuleId",
                table: "VideoResults",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CourseModuleId",
                table: "UnitResults",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CourseResultId",
                table: "UnitResults",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UnitResultId",
                table: "UnitResults",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CourseResultId",
                table: "LessonResults",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UnitModuleId",
                table: "LessonResults",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UnitResultId",
                table: "LessonResults",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LessonModuleId",
                table: "HomeWorkResults",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LessonModuleId",
                table: "ClassForumResults",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResultStatus",
                table: "ClassForumResults",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "DocumentResults",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedFullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UpdatedFullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LessonModuleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LessonResultId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentResults_Documents_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Documents",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DocumentResults_LessonModules_LessonModuleId",
                        column: x => x.LessonModuleId,
                        principalTable: "LessonModules",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DocumentResults_LessonResults_LessonResultId",
                        column: x => x.LessonResultId,
                        principalTable: "LessonResults",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VideoResults_LessonModuleId",
                table: "VideoResults",
                column: "LessonModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_UnitResults_CourseModuleId",
                table: "UnitResults",
                column: "CourseModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_UnitResults_CourseResultId",
                table: "UnitResults",
                column: "CourseResultId");

            migrationBuilder.CreateIndex(
                name: "IX_UnitResults_CreatedUserId",
                table: "UnitResults",
                column: "CreatedUserId")
                .Annotation("SqlServer:Include", new[] { "CompletionDate", "CorrectCount", "CorrectTotal", "CourseId", "CourseModuleId", "CourseResultId", "CreatedDate", "CreatedFullName", "IsDeleted", "Percent", "ProcessDate", "SkillScoresStr", "Status", "StudentId", "UnitId", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" });

            migrationBuilder.CreateIndex(
                name: "IX_UnitResults_UnitResultId",
                table: "UnitResults",
                column: "UnitResultId");

            migrationBuilder.CreateIndex(
                name: "IX_LessonResults_CourseResultId",
                table: "LessonResults",
                column: "CourseResultId");

            migrationBuilder.CreateIndex(
                name: "IX_LessonResults_CreatedUserId",
                table: "LessonResults",
                column: "CreatedUserId")
                .Annotation("SqlServer:Include", new[] { "CorrectCount", "CorrectTotal", "CourseId", "CourseResultId", "CreatedDate", "CreatedFullName", "IsDeleted", "LessonId", "Percent", "SkillScoresStr", "Status", "StudentId", "SummaryNote", "UnitId", "UnitModuleId", "UnitResultId", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" });

            migrationBuilder.CreateIndex(
                name: "IX_LessonResults_UnitModuleId",
                table: "LessonResults",
                column: "UnitModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_HomeWorkResults_LessonModuleId",
                table: "HomeWorkResults",
                column: "LessonModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_HomeWorkResults_StudentId",
                table: "HomeWorkResults",
                column: "StudentId")
                .Annotation("SqlServer:Include", new[] { "CorrectCount", "CorrectTotal", "CreatedDate", "CreatedFullName", "CreatedUserId", "HomeWorkId", "IsDeleted", "LessonModuleId", "LessonResultId", "Percent", "SkillScoresStr", "Status", "SubmissionCount", "TokenFirstTime", "TokenLastTime", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" });

            migrationBuilder.CreateIndex(
                name: "IX_ClassForumResults_LessonModuleId",
                table: "ClassForumResults",
                column: "LessonModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentResults_DocumentId",
                table: "DocumentResults",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentResults_LessonModuleId",
                table: "DocumentResults",
                column: "LessonModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentResults_LessonResultId",
                table: "DocumentResults",
                column: "LessonResultId");

            migrationBuilder.AddForeignKey(
                name: "FK_ClassForumResults_LessonModules_LessonModuleId",
                table: "ClassForumResults",
                column: "LessonModuleId",
                principalTable: "LessonModules",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_HomeWorkResults_LessonModules_LessonModuleId",
                table: "HomeWorkResults",
                column: "LessonModuleId",
                principalTable: "LessonModules",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LessonResults_CourseResults_CourseResultId",
                table: "LessonResults",
                column: "CourseResultId",
                principalTable: "CourseResults",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LessonResults_UnitModules_UnitModuleId",
                table: "LessonResults",
                column: "UnitModuleId",
                principalTable: "UnitModules",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LessonResults_UnitResults_UnitModuleId",
                table: "LessonResults",
                column: "UnitModuleId",
                principalTable: "UnitResults",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UnitResults_CourseModules_CourseModuleId",
                table: "UnitResults",
                column: "CourseModuleId",
                principalTable: "CourseModules",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UnitResults_CourseResults_CourseResultId",
                table: "UnitResults",
                column: "CourseResultId",
                principalTable: "CourseResults",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UnitResults_UnitResults_UnitResultId",
                table: "UnitResults",
                column: "UnitResultId",
                principalTable: "UnitResults",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_VideoResults_LessonModules_LessonModuleId",
                table: "VideoResults",
                column: "LessonModuleId",
                principalTable: "LessonModules",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClassForumResults_LessonModules_LessonModuleId",
                table: "ClassForumResults");

            migrationBuilder.DropForeignKey(
                name: "FK_HomeWorkResults_LessonModules_LessonModuleId",
                table: "HomeWorkResults");

            migrationBuilder.DropForeignKey(
                name: "FK_LessonResults_CourseResults_CourseResultId",
                table: "LessonResults");

            migrationBuilder.DropForeignKey(
                name: "FK_LessonResults_UnitModules_UnitModuleId",
                table: "LessonResults");

            migrationBuilder.DropForeignKey(
                name: "FK_LessonResults_UnitResults_UnitModuleId",
                table: "LessonResults");

            migrationBuilder.DropForeignKey(
                name: "FK_UnitResults_CourseModules_CourseModuleId",
                table: "UnitResults");

            migrationBuilder.DropForeignKey(
                name: "FK_UnitResults_CourseResults_CourseResultId",
                table: "UnitResults");

            migrationBuilder.DropForeignKey(
                name: "FK_UnitResults_UnitResults_UnitResultId",
                table: "UnitResults");

            migrationBuilder.DropForeignKey(
                name: "FK_VideoResults_LessonModules_LessonModuleId",
                table: "VideoResults");

            migrationBuilder.DropTable(
                name: "DocumentResults");

            migrationBuilder.DropIndex(
                name: "IX_VideoResults_LessonModuleId",
                table: "VideoResults");

            migrationBuilder.DropIndex(
                name: "IX_UnitResults_CourseModuleId",
                table: "UnitResults");

            migrationBuilder.DropIndex(
                name: "IX_UnitResults_CourseResultId",
                table: "UnitResults");

            migrationBuilder.DropIndex(
                name: "IX_UnitResults_CreatedUserId",
                table: "UnitResults");

            migrationBuilder.DropIndex(
                name: "IX_UnitResults_UnitResultId",
                table: "UnitResults");

            migrationBuilder.DropIndex(
                name: "IX_LessonResults_CourseResultId",
                table: "LessonResults");

            migrationBuilder.DropIndex(
                name: "IX_LessonResults_CreatedUserId",
                table: "LessonResults");

            migrationBuilder.DropIndex(
                name: "IX_LessonResults_UnitModuleId",
                table: "LessonResults");

            migrationBuilder.DropIndex(
                name: "IX_HomeWorkResults_LessonModuleId",
                table: "HomeWorkResults");

            migrationBuilder.DropIndex(
                name: "IX_HomeWorkResults_StudentId",
                table: "HomeWorkResults");

            migrationBuilder.DropIndex(
                name: "IX_ClassForumResults_LessonModuleId",
                table: "ClassForumResults");

            migrationBuilder.DropColumn(
                name: "LessonModuleId",
                table: "VideoResults");

            migrationBuilder.DropColumn(
                name: "CourseModuleId",
                table: "UnitResults");

            migrationBuilder.DropColumn(
                name: "CourseResultId",
                table: "UnitResults");

            migrationBuilder.DropColumn(
                name: "UnitResultId",
                table: "UnitResults");

            migrationBuilder.DropColumn(
                name: "CourseResultId",
                table: "LessonResults");

            migrationBuilder.DropColumn(
                name: "UnitModuleId",
                table: "LessonResults");

            migrationBuilder.DropColumn(
                name: "UnitResultId",
                table: "LessonResults");

            migrationBuilder.DropColumn(
                name: "LessonModuleId",
                table: "HomeWorkResults");

            migrationBuilder.DropColumn(
                name: "LessonModuleId",
                table: "ClassForumResults");

            migrationBuilder.DropColumn(
                name: "ResultStatus",
                table: "ClassForumResults");

            migrationBuilder.CreateIndex(
                name: "IX_UnitResults_CreatedUserId",
                table: "UnitResults",
                column: "CreatedUserId")
                .Annotation("SqlServer:Include", new[] { "CompletionDate", "CorrectCount", "CorrectTotal", "CourseId", "CreatedDate", "CreatedFullName", "IsDeleted", "Percent", "ProcessDate", "SkillScoresStr", "Status", "StudentId", "UnitId", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" });

            migrationBuilder.CreateIndex(
                name: "IX_LessonResults_CreatedUserId",
                table: "LessonResults",
                column: "CreatedUserId")
                .Annotation("SqlServer:Include", new[] { "CorrectCount", "CorrectTotal", "CourseId", "CreatedDate", "CreatedFullName", "IsDeleted", "LessonId", "Percent", "SkillScoresStr", "Status", "StudentId", "SummaryNote", "UnitId", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" });

            migrationBuilder.CreateIndex(
                name: "IX_HomeWorkResults_StudentId",
                table: "HomeWorkResults",
                column: "StudentId")
                .Annotation("SqlServer:Include", new[] { "CorrectCount", "CorrectTotal", "CreatedDate", "CreatedFullName", "CreatedUserId", "HomeWorkId", "IsDeleted", "LessonResultId", "Percent", "SkillScoresStr", "Status", "SubmissionCount", "TokenFirstTime", "TokenLastTime", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" });
        }
    }
}
