using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTestConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SkillId",
                table: "Sections",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QuestionName",
                table: "Questions",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ExportDistrictEvents",
                columns: table => new
                {
                    LocationName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LocationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    NumberOfCompletedLessons = table.Column<double>(type: "float", nullable: false),
                    TargetLessonCompletionRate = table.Column<double>(type: "float", nullable: false),
                    ScoreLevelLesson = table.Column<double>(type: "float", nullable: false),
                    LevelCompletionRate = table.Column<double>(type: "float", nullable: false),
                    AchievedScore = table.Column<double>(type: "float", nullable: false),
                    AssignmentClassForum = table.Column<double>(type: "float", nullable: false),
                    ScoreLevelClassForum = table.Column<double>(type: "float", nullable: false),
                    NumberofCommentsonPosts = table.Column<double>(type: "float", nullable: false),
                    ScoreLevelComment = table.Column<double>(type: "float", nullable: false),
                    TotalScore = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "ExportSchoolEvents",
                columns: table => new
                {
                    School = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NumberOfCompletedLessons = table.Column<double>(type: "float", nullable: false),
                    TargetLessonCompletionRate = table.Column<double>(type: "float", nullable: false),
                    ScoreLevelLesson = table.Column<double>(type: "float", nullable: false),
                    LevelCompletionRate = table.Column<double>(type: "float", nullable: true),
                    AchievedScore = table.Column<double>(type: "float", nullable: true),
                    AssignmentClassForum = table.Column<double>(type: "float", nullable: true),
                    ScoreLevelClassForum = table.Column<double>(type: "float", nullable: true),
                    NumberofCommentsonPosts = table.Column<double>(type: "float", nullable: true),
                    ScoreLevelComment = table.Column<double>(type: "float", nullable: true),
                    TotalScore = table.Column<double>(type: "float", nullable: true)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "ExportStudentEvents",
                columns: table => new
                {
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    School = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SchoolGrade = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SchoolClass = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NumberOfCompletedLessons = table.Column<double>(type: "float", nullable: false),
                    TargetLessonCompletionRate = table.Column<double>(type: "float", nullable: false),
                    ScoreLevelLesson = table.Column<double>(type: "float", nullable: false),
                    LevelCompletionRate = table.Column<double>(type: "float", nullable: true),
                    AchievedScore = table.Column<double>(type: "float", nullable: true),
                    AssignmentClassForum = table.Column<double>(type: "float", nullable: true),
                    ScoreLevelClassForum = table.Column<double>(type: "float", nullable: true),
                    NumberofCommentsonPosts = table.Column<double>(type: "float", nullable: true),
                    ScoreLevelComment = table.Column<double>(type: "float", nullable: true),
                    TotalScore = table.Column<double>(type: "float", nullable: true),
                    LocalId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GlobalId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EventCode = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateIndex(
                name: "IX_Sections_SkillId",
                table: "Sections",
                column: "SkillId");

            migrationBuilder.AddForeignKey(
                name: "FK_Sections_Skills_SkillId",
                table: "Sections",
                column: "SkillId",
                principalTable: "Skills",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sections_Skills_SkillId",
                table: "Sections");

            migrationBuilder.DropTable(
                name: "ExportDistrictEvents");

            migrationBuilder.DropTable(
                name: "ExportSchoolEvents");

            migrationBuilder.DropTable(
                name: "ExportStudentEvents");

            migrationBuilder.DropIndex(
                name: "IX_Sections_SkillId",
                table: "Sections");

            migrationBuilder.DropColumn(
                name: "SkillId",
                table: "Sections");

            migrationBuilder.DropColumn(
                name: "QuestionName",
                table: "Questions");
        }
    }
}
