using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TestAndPt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TestGroupResult",
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
                    Percent = table.Column<double>(type: "float", nullable: false),
                    TestType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CompletionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CourseId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CourseResultId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UnitResultId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CourseModuleId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UnitModuleId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LevelId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CurrentLevelId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FlowId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ProgramId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestGroupResult", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TestGroupResult_Categorys_ProgramId",
                        column: x => x.ProgramId,
                        principalTable: "Categorys",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TestGroupResult_CourseModules_CourseModuleId",
                        column: x => x.CourseModuleId,
                        principalTable: "CourseModules",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TestGroupResult_CourseResults_CourseResultId",
                        column: x => x.CourseResultId,
                        principalTable: "CourseResults",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TestGroupResult_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TestGroupResult_Flows_FlowId",
                        column: x => x.FlowId,
                        principalTable: "Flows",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TestGroupResult_Levels_CurrentLevelId",
                        column: x => x.CurrentLevelId,
                        principalTable: "Levels",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TestGroupResult_Levels_LevelId",
                        column: x => x.LevelId,
                        principalTable: "Levels",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TestGroupResult_UnitModules_UnitModuleId",
                        column: x => x.UnitModuleId,
                        principalTable: "UnitModules",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TestGroupResult_UnitResults_UnitResultId",
                        column: x => x.UnitResultId,
                        principalTable: "UnitResults",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TestGroupResult_Units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TestResult",
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
                    IsViewed = table.Column<bool>(type: "bit", nullable: false),
                    MaxHoursCompleted = table.Column<int>(type: "int", nullable: false),
                    TestId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TestGroupResultId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StepFlowId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ActionFlowId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CorrectCount = table.Column<int>(type: "int", nullable: false),
                    CorrectTotal = table.Column<int>(type: "int", nullable: false),
                    Percent = table.Column<double>(type: "float", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SkillScoresStr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HighestStreak = table.Column<int>(type: "int", nullable: true),
                    WorkingTime = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestResult", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TestResult_ActionFlows_ActionFlowId",
                        column: x => x.ActionFlowId,
                        principalTable: "ActionFlows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TestResult_StepFlows_StepFlowId",
                        column: x => x.StepFlowId,
                        principalTable: "StepFlows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TestResult_TestGroupResult_TestGroupResultId",
                        column: x => x.TestGroupResultId,
                        principalTable: "TestGroupResult",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TestResult_Tests_TestId",
                        column: x => x.TestId,
                        principalTable: "Tests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TestSectionResult",
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
                    TestResultId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TestSectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ParentTestSectionResultId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CorrectCount = table.Column<int>(type: "int", nullable: false),
                    CorrectTotal = table.Column<int>(type: "int", nullable: false),
                    Percent = table.Column<double>(type: "float", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SkillScoresStr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HighestStreak = table.Column<int>(type: "int", nullable: true),
                    WorkingTime = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestSectionResult", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TestSectionResult_TestResult_TestResultId",
                        column: x => x.TestResultId,
                        principalTable: "TestResult",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TestSectionResult_TestSectionResult_ParentTestSectionResultId",
                        column: x => x.ParentTestSectionResultId,
                        principalTable: "TestSectionResult",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TestSectionResult_TestSections_TestSectionId",
                        column: x => x.TestSectionId,
                        principalTable: "TestSections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TestAnswer",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedFullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    GradingAlFeedback = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TimeCount = table.Column<int>(type: "int", nullable: true),
                    WordCount = table.Column<int>(type: "int", nullable: true),
                    SpeechTextAnswer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PronunciationScore = table.Column<double>(type: "float", nullable: true),
                    RetryTime = table.Column<int>(type: "int", nullable: false),
                    TestSectionResultId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    QuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TestResultId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AnswerStr = table.Column<string>(type: "nvarchar(max)", maxLength: 11000, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsCorrect = table.Column<bool>(type: "bit", nullable: true),
                    CorrectCount = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestAnswer", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TestAnswer_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TestAnswer_TestResult_TestResultId",
                        column: x => x.TestResultId,
                        principalTable: "TestResult",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TestAnswer_TestSectionResult_TestSectionResultId",
                        column: x => x.TestSectionResultId,
                        principalTable: "TestSectionResult",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TestAnswer_QuestionId",
                table: "TestAnswer",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_TestAnswer_TestResultId",
                table: "TestAnswer",
                column: "TestResultId");

            migrationBuilder.CreateIndex(
                name: "IX_TestAnswer_TestSectionResultId",
                table: "TestAnswer",
                column: "TestSectionResultId");

            migrationBuilder.CreateIndex(
                name: "IX_TestGroupResult_CourseId",
                table: "TestGroupResult",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_TestGroupResult_CourseModuleId",
                table: "TestGroupResult",
                column: "CourseModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_TestGroupResult_CourseResultId",
                table: "TestGroupResult",
                column: "CourseResultId");

            migrationBuilder.CreateIndex(
                name: "IX_TestGroupResult_CurrentLevelId",
                table: "TestGroupResult",
                column: "CurrentLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_TestGroupResult_FlowId",
                table: "TestGroupResult",
                column: "FlowId");

            migrationBuilder.CreateIndex(
                name: "IX_TestGroupResult_LevelId",
                table: "TestGroupResult",
                column: "LevelId");

            migrationBuilder.CreateIndex(
                name: "IX_TestGroupResult_ProgramId",
                table: "TestGroupResult",
                column: "ProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_TestGroupResult_UnitId",
                table: "TestGroupResult",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_TestGroupResult_UnitModuleId",
                table: "TestGroupResult",
                column: "UnitModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_TestGroupResult_UnitResultId",
                table: "TestGroupResult",
                column: "UnitResultId");

            migrationBuilder.CreateIndex(
                name: "IX_TestResult_ActionFlowId",
                table: "TestResult",
                column: "ActionFlowId");

            migrationBuilder.CreateIndex(
                name: "IX_TestResult_StepFlowId",
                table: "TestResult",
                column: "StepFlowId");

            migrationBuilder.CreateIndex(
                name: "IX_TestResult_TestGroupResultId",
                table: "TestResult",
                column: "TestGroupResultId");

            migrationBuilder.CreateIndex(
                name: "IX_TestResult_TestId",
                table: "TestResult",
                column: "TestId");

            migrationBuilder.CreateIndex(
                name: "IX_TestSectionResult_ParentTestSectionResultId",
                table: "TestSectionResult",
                column: "ParentTestSectionResultId");

            migrationBuilder.CreateIndex(
                name: "IX_TestSectionResult_TestResultId",
                table: "TestSectionResult",
                column: "TestResultId");

            migrationBuilder.CreateIndex(
                name: "IX_TestSectionResult_TestSectionId",
                table: "TestSectionResult",
                column: "TestSectionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TestAnswer");

            migrationBuilder.DropTable(
                name: "TestSectionResult");

            migrationBuilder.DropTable(
                name: "TestResult");

            migrationBuilder.DropTable(
                name: "TestGroupResult");
        }
    }
}
