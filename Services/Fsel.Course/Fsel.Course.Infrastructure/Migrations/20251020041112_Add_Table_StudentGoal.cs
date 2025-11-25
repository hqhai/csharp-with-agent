using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_Table_StudentGoal : Migration
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

            migrationBuilder.AddColumn<DateTime>(
                name: "NewDate",
                table: "UnitResults",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletionDate",
                table: "MockTestResults",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NewDate",
                table: "MockTestResults",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ProcessDate",
                table: "MockTestResults",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletionDate",
                table: "LessonResults",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NewDate",
                table: "LessonResults",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ProcessDate",
                table: "LessonResults",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletionDate",
                table: "HomeWorkResults",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NewDate",
                table: "HomeWorkResults",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ProcessDate",
                table: "HomeWorkResults",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletionDate",
                table: "FinalTestResults",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NewDate",
                table: "FinalTestResults",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ProcessDate",
                table: "FinalTestResults",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NewDate",
                table: "CourseResults",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "StudentGoalAggregates",
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
                    TotalCompletedLessons = table.Column<int>(type: "int", nullable: false),
                    TotalTargetLessons = table.Column<int>(type: "int", nullable: false),
                    ConsecutiveBehindWeeks = table.Column<int>(type: "int", nullable: false),
                    CombinedProgress = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CourseLevel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CourseType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SchoolName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    ClassName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    ClassId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SchoolId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CourseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CourseGoalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CourseGoalConfigId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentGoalAggregates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentGoalAggregates_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StudentGoalSummaries",
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
                    TotalCompletedLessons = table.Column<int>(type: "int", nullable: false),
                    TotalTargetLessons = table.Column<int>(type: "int", nullable: false),
                    LessonsPerWeek = table.Column<int>(type: "int", nullable: false),
                    CompletedLessons = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastCompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ProgressStatus = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    StudentGoalAggregateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentGoalSummaries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentGoalSummaries_StudentGoalAggregates_StudentGoalAggregateId",
                        column: x => x.StudentGoalAggregateId,
                        principalTable: "StudentGoalAggregates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UnitResults_CreatedUserId",
                table: "UnitResults",
                column: "CreatedUserId")
                .Annotation("SqlServer:Include", new[] { "CompletionDate", "CorrectCount", "CorrectTotal", "CourseId", "CreatedDate", "CreatedFullName", "IsDeleted", "NewDate", "Percent", "ProcessDate", "SkillScoresStr", "Status", "StudentId", "UnitId", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" });

            migrationBuilder.CreateIndex(
                name: "IX_LessonResults_CreatedUserId",
                table: "LessonResults",
                column: "CreatedUserId")
                .Annotation("SqlServer:Include", new[] { "CompletionDate", "CorrectCount", "CorrectTotal", "CourseId", "CreatedDate", "CreatedFullName", "IsDeleted", "LessonId", "NewDate", "Percent", "ProcessDate", "SkillScoresStr", "Status", "StudentId", "SummaryNote", "UnitId", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" });

            migrationBuilder.CreateIndex(
                name: "IX_HomeWorkResults_StudentId",
                table: "HomeWorkResults",
                column: "StudentId")
                .Annotation("SqlServer:Include", new[] { "CompletionDate", "CorrectCount", "CorrectTotal", "CreatedDate", "CreatedFullName", "CreatedUserId", "HomeWorkId", "IsDeleted", "LessonResultId", "NewDate", "Percent", "ProcessDate", "SkillScoresStr", "Status", "SubmissionCount", "TokenFirstTime", "TokenLastTime", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" });

            migrationBuilder.CreateIndex(
                name: "IX_StudentGoalAggregates_CourseId",
                table: "StudentGoalAggregates",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentGoalSummaries_StudentGoalAggregateId",
                table: "StudentGoalSummaries",
                column: "StudentGoalAggregateId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StudentGoalSummaries");

            migrationBuilder.DropTable(
                name: "StudentGoalAggregates");

            migrationBuilder.DropIndex(
                name: "IX_UnitResults_CreatedUserId",
                table: "UnitResults");

            migrationBuilder.DropIndex(
                name: "IX_LessonResults_CreatedUserId",
                table: "LessonResults");

            migrationBuilder.DropIndex(
                name: "IX_HomeWorkResults_StudentId",
                table: "HomeWorkResults");

            migrationBuilder.DropColumn(
                name: "NewDate",
                table: "UnitResults");

            migrationBuilder.DropColumn(
                name: "CompletionDate",
                table: "MockTestResults");

            migrationBuilder.DropColumn(
                name: "NewDate",
                table: "MockTestResults");

            migrationBuilder.DropColumn(
                name: "ProcessDate",
                table: "MockTestResults");

            migrationBuilder.DropColumn(
                name: "CompletionDate",
                table: "LessonResults");

            migrationBuilder.DropColumn(
                name: "NewDate",
                table: "LessonResults");

            migrationBuilder.DropColumn(
                name: "ProcessDate",
                table: "LessonResults");

            migrationBuilder.DropColumn(
                name: "CompletionDate",
                table: "HomeWorkResults");

            migrationBuilder.DropColumn(
                name: "NewDate",
                table: "HomeWorkResults");

            migrationBuilder.DropColumn(
                name: "ProcessDate",
                table: "HomeWorkResults");

            migrationBuilder.DropColumn(
                name: "CompletionDate",
                table: "FinalTestResults");

            migrationBuilder.DropColumn(
                name: "NewDate",
                table: "FinalTestResults");

            migrationBuilder.DropColumn(
                name: "ProcessDate",
                table: "FinalTestResults");

            migrationBuilder.DropColumn(
                name: "NewDate",
                table: "CourseResults");

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
