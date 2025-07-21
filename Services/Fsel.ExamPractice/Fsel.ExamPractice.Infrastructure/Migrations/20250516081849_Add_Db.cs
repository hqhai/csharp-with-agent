using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.ExamPractice.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_Db : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExamPractices",
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
                    Name = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SchoolGrade = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SubType = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Province = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ProvinceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ExecutionTime = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CourseLevel = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ActivatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ParentExamPracticeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamPractices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamPractices_ExamPractices_ParentExamPracticeId",
                        column: x => x.ParentExamPracticeId,
                        principalTable: "ExamPractices",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ExamPracticeRetrys",
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
                    RetryCount = table.Column<int>(type: "int", nullable: false),
                    ExamPracticeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamPracticeRetrys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamPracticeRetrys_ExamPractices_ExamPracticeId",
                        column: x => x.ExamPracticeId,
                        principalTable: "ExamPractices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExamPracticeSections",
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
                    Name = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    CourseSkill = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ConfigStr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    ExamPracticeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ParentExamPracticeSectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamPracticeSections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamPracticeSections_ExamPracticeSections_ParentExamPracticeSectionId",
                        column: x => x.ParentExamPracticeSectionId,
                        principalTable: "ExamPracticeSections",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ExamPracticeSections_ExamPractices_ExamPracticeId",
                        column: x => x.ExamPracticeId,
                        principalTable: "ExamPractices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExamPracticeResults",
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
                    CorrectCount = table.Column<int>(type: "int", nullable: false),
                    CorrectTotal = table.Column<int>(type: "int", nullable: false),
                    Percent = table.Column<double>(type: "float", nullable: false),
                    SkillScoresStr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PracticeMode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ExerciseConfig = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HighestStreak = table.Column<int>(type: "int", nullable: true),
                    WorkingTime = table.Column<double>(type: "float", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ResultPosition = table.Column<int>(type: "int", nullable: false),
                    ExamPracticeRetryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExamPracticeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamPracticeResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamPracticeResults_ExamPracticeRetrys_ExamPracticeRetryId",
                        column: x => x.ExamPracticeRetryId,
                        principalTable: "ExamPracticeRetrys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExamPracticeResults_ExamPractices_ExamPracticeId",
                        column: x => x.ExamPracticeId,
                        principalTable: "ExamPractices",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ExamPracticeAISettings",
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
                    SystemRoleAlConfig = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserAlConfig = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SettingModel = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    SettingTemperature = table.Column<double>(type: "float", nullable: false),
                    SettingWordMaxLength = table.Column<double>(type: "float", nullable: false),
                    SettingTopP = table.Column<double>(type: "float", nullable: false),
                    SettingFrequecy = table.Column<double>(type: "float", nullable: false),
                    SettingPresence = table.Column<double>(type: "float", nullable: false),
                    Task = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PromptStr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExamPracticeSectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamPracticeAISettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamPracticeAISettings_ExamPracticeSections_ExamPracticeSectionId",
                        column: x => x.ExamPracticeSectionId,
                        principalTable: "ExamPracticeSections",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Questions",
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
                    QuestionType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Ungraded = table.Column<bool>(type: "bit", nullable: false),
                    Explanation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CorrectTotal = table.Column<int>(type: "int", nullable: false),
                    ConfigStr = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SubQuestionIndexsStr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: true),
                    ExamPracticeSectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Questions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Questions_ExamPracticeSections_ExamPracticeSectionId",
                        column: x => x.ExamPracticeSectionId,
                        principalTable: "ExamPracticeSections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExamPracticeSectionResults",
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
                    CorrectCount = table.Column<int>(type: "int", nullable: false),
                    CorrectTotal = table.Column<int>(type: "int", nullable: false),
                    Percent = table.Column<double>(type: "float", nullable: false),
                    SkillScoresStr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HighestStreak = table.Column<int>(type: "int", nullable: true),
                    WorkingTime = table.Column<double>(type: "float", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExamPracticeResultId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParentExamPracticeSectionResultId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamPracticeSectionResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamPracticeSectionResults_ExamPracticeResults_ExamPracticeResultId",
                        column: x => x.ExamPracticeResultId,
                        principalTable: "ExamPracticeResults",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExamPracticeSectionResults_ExamPracticeSectionResults_ParentExamPracticeSectionResultId",
                        column: x => x.ParentExamPracticeSectionResultId,
                        principalTable: "ExamPracticeSectionResults",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ExamPracticeAICriteriaSettings",
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
                    SystemRoleAlConfig = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PromptStr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CriteriaName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ExamPracticeAISettingId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamPracticeAICriteriaSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamPracticeAICriteriaSettings_ExamPracticeAISettings_ExamPracticeAISettingId",
                        column: x => x.ExamPracticeAISettingId,
                        principalTable: "ExamPracticeAISettings",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ExamPracticeAnswers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedFullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsCorrect = table.Column<bool>(type: "bit", nullable: true),
                    CorrectCount = table.Column<short>(type: "smallint", nullable: false),
                    AnswerStr = table.Column<string>(type: "nvarchar(max)", maxLength: 11000, nullable: true),
                    TimeCount = table.Column<int>(type: "int", nullable: true),
                    WordCount = table.Column<int>(type: "int", nullable: true),
                    GradingAlFeedback = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SpeechTextAnswer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PronunciationScore = table.Column<double>(type: "float", nullable: true),
                    RetryTime = table.Column<int>(type: "int", nullable: false),
                    ExamPracticeResultId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExamPracticeSectionResultId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamPracticeAnswers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamPracticeAnswers_ExamPracticeResults_ExamPracticeResultId",
                        column: x => x.ExamPracticeResultId,
                        principalTable: "ExamPracticeResults",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ExamPracticeAnswers_ExamPracticeSectionResults_ExamPracticeSectionResultId",
                        column: x => x.ExamPracticeSectionResultId,
                        principalTable: "ExamPracticeSectionResults",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExamPracticeAICriteriaSettings_ExamPracticeAISettingId",
                table: "ExamPracticeAICriteriaSettings",
                column: "ExamPracticeAISettingId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamPracticeAISettings_ExamPracticeSectionId",
                table: "ExamPracticeAISettings",
                column: "ExamPracticeSectionId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamPracticeAnswers_ExamPracticeResultId",
                table: "ExamPracticeAnswers",
                column: "ExamPracticeResultId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamPracticeAnswers_ExamPracticeSectionResultId",
                table: "ExamPracticeAnswers",
                column: "ExamPracticeSectionResultId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamPracticeResults_ExamPracticeId",
                table: "ExamPracticeResults",
                column: "ExamPracticeId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamPracticeResults_ExamPracticeRetryId",
                table: "ExamPracticeResults",
                column: "ExamPracticeRetryId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamPracticeRetrys_ExamPracticeId",
                table: "ExamPracticeRetrys",
                column: "ExamPracticeId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamPractices_ParentExamPracticeId",
                table: "ExamPractices",
                column: "ParentExamPracticeId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamPracticeSectionResults_ExamPracticeResultId",
                table: "ExamPracticeSectionResults",
                column: "ExamPracticeResultId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamPracticeSectionResults_ParentExamPracticeSectionResultId",
                table: "ExamPracticeSectionResults",
                column: "ParentExamPracticeSectionResultId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamPracticeSections_ExamPracticeId",
                table: "ExamPracticeSections",
                column: "ExamPracticeId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamPracticeSections_ParentExamPracticeSectionId",
                table: "ExamPracticeSections",
                column: "ParentExamPracticeSectionId");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_ExamPracticeSectionId",
                table: "Questions",
                column: "ExamPracticeSectionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExamPracticeAICriteriaSettings");

            migrationBuilder.DropTable(
                name: "ExamPracticeAnswers");

            migrationBuilder.DropTable(
                name: "Questions");

            migrationBuilder.DropTable(
                name: "ExamPracticeAISettings");

            migrationBuilder.DropTable(
                name: "ExamPracticeSectionResults");

            migrationBuilder.DropTable(
                name: "ExamPracticeSections");

            migrationBuilder.DropTable(
                name: "ExamPracticeResults");

            migrationBuilder.DropTable(
                name: "ExamPracticeRetrys");

            migrationBuilder.DropTable(
                name: "ExamPractices");
        }
    }
}
