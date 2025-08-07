using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_Optimize_Field_Delete_Result : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UnitResults_CreatedUserId",
                table: "UnitResults");

            migrationBuilder.DropIndex(
                name: "IX_PlacementTestResults_CreatedUserId",
                table: "PlacementTestResults");

            migrationBuilder.DropIndex(
                name: "IX_LessonResults_CreatedUserId",
                table: "LessonResults");

            migrationBuilder.DropIndex(
                name: "IX_HomeWorkResults_StudentId",
                table: "HomeWorkResults");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                table: "VideoTimeCodeResults");

            migrationBuilder.DropColumn(
                name: "DeletedFullName",
                table: "VideoTimeCodeResults");

            migrationBuilder.DropColumn(
                name: "DeletedUserId",
                table: "VideoTimeCodeResults");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                table: "VideoResults");

            migrationBuilder.DropColumn(
                name: "DeletedFullName",
                table: "VideoResults");

            migrationBuilder.DropColumn(
                name: "DeletedUserId",
                table: "VideoResults");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                table: "UnitResults");

            migrationBuilder.DropColumn(
                name: "DeletedFullName",
                table: "UnitResults");

            migrationBuilder.DropColumn(
                name: "DeletedUserId",
                table: "UnitResults");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                table: "SectionGroupResults");

            migrationBuilder.DropColumn(
                name: "DeletedFullName",
                table: "SectionGroupResults");

            migrationBuilder.DropColumn(
                name: "DeletedUserId",
                table: "SectionGroupResults");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                table: "PlacementTestResults");

            migrationBuilder.DropColumn(
                name: "DeletedFullName",
                table: "PlacementTestResults");

            migrationBuilder.DropColumn(
                name: "DeletedUserId",
                table: "PlacementTestResults");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                table: "MockTestResults");

            migrationBuilder.DropColumn(
                name: "DeletedFullName",
                table: "MockTestResults");

            migrationBuilder.DropColumn(
                name: "DeletedUserId",
                table: "MockTestResults");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                table: "LessonResults");

            migrationBuilder.DropColumn(
                name: "DeletedFullName",
                table: "LessonResults");

            migrationBuilder.DropColumn(
                name: "DeletedUserId",
                table: "LessonResults");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                table: "HomeWorkResults");

            migrationBuilder.DropColumn(
                name: "DeletedFullName",
                table: "HomeWorkResults");

            migrationBuilder.DropColumn(
                name: "DeletedUserId",
                table: "HomeWorkResults");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                table: "FinalTestResults");

            migrationBuilder.DropColumn(
                name: "DeletedFullName",
                table: "FinalTestResults");

            migrationBuilder.DropColumn(
                name: "DeletedUserId",
                table: "FinalTestResults");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                table: "ExtraPracticeResults");

            migrationBuilder.DropColumn(
                name: "DeletedFullName",
                table: "ExtraPracticeResults");

            migrationBuilder.DropColumn(
                name: "DeletedUserId",
                table: "ExtraPracticeResults");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                table: "ExtraPracticeExerciseResults");

            migrationBuilder.DropColumn(
                name: "DeletedFullName",
                table: "ExtraPracticeExerciseResults");

            migrationBuilder.DropColumn(
                name: "DeletedUserId",
                table: "ExtraPracticeExerciseResults");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                table: "CourseResults");

            migrationBuilder.DropColumn(
                name: "DeletedFullName",
                table: "CourseResults");

            migrationBuilder.DropColumn(
                name: "DeletedUserId",
                table: "CourseResults");

            migrationBuilder.CreateIndex(
                name: "IX_UnitResults_CreatedUserId",
                table: "UnitResults",
                column: "CreatedUserId")
                .Annotation("SqlServer:Include", new[] { "CompletionDate", "CorrectCount", "CorrectTotal", "CourseId", "CreatedDate", "CreatedFullName", "IsDeleted", "Percent", "ProcessDate", "SkillScoresStr", "Status", "StudentId", "UnitId", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" });

            migrationBuilder.CreateIndex(
                name: "IX_PlacementTestResults_CreatedUserId",
                table: "PlacementTestResults",
                column: "CreatedUserId")
                .Annotation("SqlServer:Include", new[] { "CorrectCount", "CorrectTotal", "CountQuestion", "CreatedDate", "CreatedFullName", "IsDeleted", "Level", "Percent", "PlacementTestGroupResultId", "PlacementTestId", "SkillScoresStr", "Status", "StudentId", "TotalQuestion", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" });

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UnitResults_CreatedUserId",
                table: "UnitResults");

            migrationBuilder.DropIndex(
                name: "IX_PlacementTestResults_CreatedUserId",
                table: "PlacementTestResults");

            migrationBuilder.DropIndex(
                name: "IX_LessonResults_CreatedUserId",
                table: "LessonResults");

            migrationBuilder.DropIndex(
                name: "IX_HomeWorkResults_StudentId",
                table: "HomeWorkResults");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDate",
                table: "VideoTimeCodeResults",
                type: "datetime2",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 109);

            migrationBuilder.AddColumn<string>(
                name: "DeletedFullName",
                table: "VideoTimeCodeResults",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("Relational:ColumnOrder", 106);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedUserId",
                table: "VideoTimeCodeResults",
                type: "uniqueidentifier",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 103);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDate",
                table: "VideoResults",
                type: "datetime2",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 109);

            migrationBuilder.AddColumn<string>(
                name: "DeletedFullName",
                table: "VideoResults",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("Relational:ColumnOrder", 106);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedUserId",
                table: "VideoResults",
                type: "uniqueidentifier",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 103);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDate",
                table: "UnitResults",
                type: "datetime2",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 109);

            migrationBuilder.AddColumn<string>(
                name: "DeletedFullName",
                table: "UnitResults",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("Relational:ColumnOrder", 106);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedUserId",
                table: "UnitResults",
                type: "uniqueidentifier",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 103);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDate",
                table: "SectionGroupResults",
                type: "datetime2",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 109);

            migrationBuilder.AddColumn<string>(
                name: "DeletedFullName",
                table: "SectionGroupResults",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("Relational:ColumnOrder", 106);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedUserId",
                table: "SectionGroupResults",
                type: "uniqueidentifier",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 103);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDate",
                table: "PlacementTestResults",
                type: "datetime2",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 109);

            migrationBuilder.AddColumn<string>(
                name: "DeletedFullName",
                table: "PlacementTestResults",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("Relational:ColumnOrder", 106);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedUserId",
                table: "PlacementTestResults",
                type: "uniqueidentifier",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 103);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDate",
                table: "MockTestResults",
                type: "datetime2",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 109);

            migrationBuilder.AddColumn<string>(
                name: "DeletedFullName",
                table: "MockTestResults",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("Relational:ColumnOrder", 106);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedUserId",
                table: "MockTestResults",
                type: "uniqueidentifier",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 103);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDate",
                table: "LessonResults",
                type: "datetime2",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 109);

            migrationBuilder.AddColumn<string>(
                name: "DeletedFullName",
                table: "LessonResults",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("Relational:ColumnOrder", 106);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedUserId",
                table: "LessonResults",
                type: "uniqueidentifier",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 103);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDate",
                table: "HomeWorkResults",
                type: "datetime2",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 109);

            migrationBuilder.AddColumn<string>(
                name: "DeletedFullName",
                table: "HomeWorkResults",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("Relational:ColumnOrder", 106);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedUserId",
                table: "HomeWorkResults",
                type: "uniqueidentifier",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 103);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDate",
                table: "FinalTestResults",
                type: "datetime2",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 109);

            migrationBuilder.AddColumn<string>(
                name: "DeletedFullName",
                table: "FinalTestResults",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("Relational:ColumnOrder", 106);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedUserId",
                table: "FinalTestResults",
                type: "uniqueidentifier",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 103);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDate",
                table: "ExtraPracticeResults",
                type: "datetime2",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 109);

            migrationBuilder.AddColumn<string>(
                name: "DeletedFullName",
                table: "ExtraPracticeResults",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("Relational:ColumnOrder", 106);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedUserId",
                table: "ExtraPracticeResults",
                type: "uniqueidentifier",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 103);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDate",
                table: "ExtraPracticeExerciseResults",
                type: "datetime2",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 109);

            migrationBuilder.AddColumn<string>(
                name: "DeletedFullName",
                table: "ExtraPracticeExerciseResults",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("Relational:ColumnOrder", 106);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedUserId",
                table: "ExtraPracticeExerciseResults",
                type: "uniqueidentifier",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 103);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDate",
                table: "CourseResults",
                type: "datetime2",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 109);

            migrationBuilder.AddColumn<string>(
                name: "DeletedFullName",
                table: "CourseResults",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("Relational:ColumnOrder", 106);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedUserId",
                table: "CourseResults",
                type: "uniqueidentifier",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 103);

            migrationBuilder.CreateIndex(
                name: "IX_UnitResults_CreatedUserId",
                table: "UnitResults",
                column: "CreatedUserId")
                .Annotation("SqlServer:Include", new[] { "CompletionDate", "CorrectCount", "CorrectTotal", "CourseId", "CreatedDate", "CreatedFullName", "DeletedDate", "DeletedFullName", "DeletedUserId", "IsDeleted", "Percent", "ProcessDate", "SkillScoresStr", "Status", "StudentId", "UnitId", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" });

            migrationBuilder.CreateIndex(
                name: "IX_PlacementTestResults_CreatedUserId",
                table: "PlacementTestResults",
                column: "CreatedUserId")
                .Annotation("SqlServer:Include", new[] { "CorrectCount", "CorrectTotal", "CountQuestion", "CreatedDate", "CreatedFullName", "DeletedDate", "DeletedFullName", "DeletedUserId", "IsDeleted", "Level", "Percent", "PlacementTestGroupResultId", "PlacementTestId", "SkillScoresStr", "Status", "StudentId", "TotalQuestion", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" });

            migrationBuilder.CreateIndex(
                name: "IX_LessonResults_CreatedUserId",
                table: "LessonResults",
                column: "CreatedUserId")
                .Annotation("SqlServer:Include", new[] { "CorrectCount", "CorrectTotal", "CourseId", "CreatedDate", "CreatedFullName", "DeletedDate", "DeletedFullName", "DeletedUserId", "IsDeleted", "LessonId", "Percent", "SkillScoresStr", "Status", "StudentId", "SummaryNote", "UnitId", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" });

            migrationBuilder.CreateIndex(
                name: "IX_HomeWorkResults_StudentId",
                table: "HomeWorkResults",
                column: "StudentId")
                .Annotation("SqlServer:Include", new[] { "CorrectCount", "CorrectTotal", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "HomeWorkId", "IsDeleted", "LessonResultId", "Percent", "SkillScoresStr", "Status", "SubmissionCount", "TokenFirstTime", "TokenLastTime", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" });
        }
    }
}
