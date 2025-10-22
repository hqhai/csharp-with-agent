using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_HomeWorkExtra_Result : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HomeWorkRetries",
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
                    NumberRetry = table.Column<int>(type: "int", nullable: false),
                    CurriculumId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    HomeWorkId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HomeWorkRetries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HomeWorkRetries_CurriculumConfigs_CurriculumId",
                        column: x => x.CurriculumId,
                        principalTable: "CurriculumConfigs",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_HomeWorkRetries_HomeWorks_HomeWorkId",
                        column: x => x.HomeWorkId,
                        principalTable: "HomeWorks",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "HomeWorkExtraPracticeResults",
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
                    SubmissionCount = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    WorkingStatus = table.Column<int>(type: "int", nullable: false),
                    HomeWorkId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HomeWorkRetryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CorrectCount = table.Column<int>(type: "int", nullable: false),
                    CorrectTotal = table.Column<int>(type: "int", nullable: false),
                    Percent = table.Column<double>(type: "float", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SkillScoresStr = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HomeWorkExtraPracticeResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HomeWorkExtraPracticeResults_HomeWorkRetries_HomeWorkRetryId",
                        column: x => x.HomeWorkRetryId,
                        principalTable: "HomeWorkRetries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HomeWorkExtraPracticeResults_HomeWorks_HomeWorkId",
                        column: x => x.HomeWorkId,
                        principalTable: "HomeWorks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HomeWorkExtraPracticeAnswers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedFullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HomeWorkExtraPracticeResultId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AnswerStr = table.Column<string>(type: "nvarchar(max)", maxLength: 11000, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsCorrect = table.Column<bool>(type: "bit", nullable: true),
                    CorrectCount = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HomeWorkExtraPracticeAnswers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HomeWorkExtraPracticeAnswers_HomeWorkExtraPracticeResults_HomeWorkExtraPracticeResultId",
                        column: x => x.HomeWorkExtraPracticeResultId,
                        principalTable: "HomeWorkExtraPracticeResults",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_HomeWorkExtraPracticeAnswers_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HomeWorkExtraPracticeAnswers_HomeWorkExtraPracticeResultId",
                table: "HomeWorkExtraPracticeAnswers",
                column: "HomeWorkExtraPracticeResultId");

            migrationBuilder.CreateIndex(
                name: "IX_HomeWorkExtraPracticeAnswers_QuestionId",
                table: "HomeWorkExtraPracticeAnswers",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_HomeWorkExtraPracticeResults_HomeWorkId",
                table: "HomeWorkExtraPracticeResults",
                column: "HomeWorkId");

            migrationBuilder.CreateIndex(
                name: "IX_HomeWorkExtraPracticeResults_HomeWorkRetryId",
                table: "HomeWorkExtraPracticeResults",
                column: "HomeWorkRetryId");

            migrationBuilder.CreateIndex(
                name: "IX_HomeWorkRetries_CurriculumId",
                table: "HomeWorkRetries",
                column: "CurriculumId");

            migrationBuilder.CreateIndex(
                name: "IX_HomeWorkRetries_HomeWorkId",
                table: "HomeWorkRetries",
                column: "HomeWorkId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HomeWorkExtraPracticeAnswers");

            migrationBuilder.DropTable(
                name: "HomeWorkExtraPracticeResults");

            migrationBuilder.DropTable(
                name: "HomeWorkRetries");
        }
    }
}
