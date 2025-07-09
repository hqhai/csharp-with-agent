using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_UnitModule_And_Edit_Unit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExportDistrictEvents");

            migrationBuilder.DropTable(
                name: "ExportSchoolEvents");

            migrationBuilder.DropTable(
                name: "ExportStudentEvents");

            migrationBuilder.AddColumn<string>(
                name: "HighlightRange",
                table: "Units",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LessonCount",
                table: "Units",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "LevelId",
                table: "Units",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OriginalId",
                table: "Units",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ProgramId",
                table: "Units",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TestCount",
                table: "Units",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "Units",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "VersionStatus",
                table: "Units",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "LessonModules",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000);

            migrationBuilder.CreateTable(
                name: "UnitModules",
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
                    UnitConfigType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    DisplayNumber = table.Column<int>(type: "int", nullable: false),
                    Percent = table.Column<double>(type: "float", nullable: false),
                    OpenOrder = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OriginalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnitModules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UnitModules_Units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Units_LevelId",
                table: "Units",
                column: "LevelId");

            migrationBuilder.CreateIndex(
                name: "IX_Units_ProgramId",
                table: "Units",
                column: "ProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_UnitModules_OriginalId",
                table: "UnitModules",
                column: "OriginalId");

            migrationBuilder.CreateIndex(
                name: "IX_UnitModules_UnitId",
                table: "UnitModules",
                column: "UnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_Units_Categorys_ProgramId",
                table: "Units",
                column: "ProgramId",
                principalTable: "Categorys",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Units_Levels_LevelId",
                table: "Units",
                column: "LevelId",
                principalTable: "Levels",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Units_Categorys_ProgramId",
                table: "Units");

            migrationBuilder.DropForeignKey(
                name: "FK_Units_Levels_LevelId",
                table: "Units");

            migrationBuilder.DropTable(
                name: "UnitModules");

            migrationBuilder.DropIndex(
                name: "IX_Units_LevelId",
                table: "Units");

            migrationBuilder.DropIndex(
                name: "IX_Units_ProgramId",
                table: "Units");

            migrationBuilder.DropColumn(
                name: "HighlightRange",
                table: "Units");

            migrationBuilder.DropColumn(
                name: "LessonCount",
                table: "Units");

            migrationBuilder.DropColumn(
                name: "LevelId",
                table: "Units");

            migrationBuilder.DropColumn(
                name: "OriginalId",
                table: "Units");

            migrationBuilder.DropColumn(
                name: "ProgramId",
                table: "Units");

            migrationBuilder.DropColumn(
                name: "TestCount",
                table: "Units");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "Units");

            migrationBuilder.DropColumn(
                name: "VersionStatus",
                table: "Units");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "LessonModules",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "ExportDistrictEvents",
                columns: table => new
                {
                    AchievedScore = table.Column<double>(type: "float", nullable: false),
                    AssignmentClassForum = table.Column<double>(type: "float", nullable: false),
                    LevelCompletionRate = table.Column<double>(type: "float", nullable: false),
                    LocationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LocationName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NumberOfCompletedLessons = table.Column<double>(type: "float", nullable: false),
                    NumberofCommentsonPosts = table.Column<double>(type: "float", nullable: false),
                    ScoreLevelClassForum = table.Column<double>(type: "float", nullable: false),
                    ScoreLevelComment = table.Column<double>(type: "float", nullable: false),
                    ScoreLevelLesson = table.Column<double>(type: "float", nullable: false),
                    TargetLessonCompletionRate = table.Column<double>(type: "float", nullable: false),
                    TotalScore = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "ExportSchoolEvents",
                columns: table => new
                {
                    AchievedScore = table.Column<double>(type: "float", nullable: true),
                    AssignmentClassForum = table.Column<double>(type: "float", nullable: true),
                    LevelCompletionRate = table.Column<double>(type: "float", nullable: true),
                    NumberOfCompletedLessons = table.Column<double>(type: "float", nullable: false),
                    NumberofCommentsonPosts = table.Column<double>(type: "float", nullable: true),
                    School = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ScoreLevelClassForum = table.Column<double>(type: "float", nullable: true),
                    ScoreLevelComment = table.Column<double>(type: "float", nullable: true),
                    ScoreLevelLesson = table.Column<double>(type: "float", nullable: false),
                    TargetLessonCompletionRate = table.Column<double>(type: "float", nullable: false),
                    TotalScore = table.Column<double>(type: "float", nullable: true)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "ExportStudentEvents",
                columns: table => new
                {
                    AchievedScore = table.Column<double>(type: "float", nullable: true),
                    AssignmentClassForum = table.Column<double>(type: "float", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EventCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GlobalId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LevelCompletionRate = table.Column<double>(type: "float", nullable: true),
                    LocalId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NumberOfCompletedLessons = table.Column<double>(type: "float", nullable: false),
                    NumberofCommentsonPosts = table.Column<double>(type: "float", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    School = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SchoolClass = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SchoolGrade = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ScoreLevelClassForum = table.Column<double>(type: "float", nullable: true),
                    ScoreLevelComment = table.Column<double>(type: "float", nullable: true),
                    ScoreLevelLesson = table.Column<double>(type: "float", nullable: false),
                    TargetLessonCompletionRate = table.Column<double>(type: "float", nullable: false),
                    TotalScore = table.Column<double>(type: "float", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                });
        }
    }
}
