using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.ExamPractice.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Create_Table_Score : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "ParentExamPracticeSectionResultId",
                table: "ExamPracticeSectionResults",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<Guid>(
                name: "CurrentExamPracticeSectionId",
                table: "ExamPracticeSectionResults",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ExamPracticeSectionId",
                table: "ExamPracticeSectionResults",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "WorkingStatus",
                table: "ExamPracticeResults",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "AnswerStr",
                table: "ExamPracticeAnswers",
                type: "nvarchar(max)",
                maxLength: 11000,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldMaxLength: 11000,
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ExamPracticeSectionId",
                table: "ExamPracticeAnswers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "QuestionId",
                table: "ExamPracticeAnswers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ExamPracticeScores",
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
                    Criteria = table.Column<int>(type: "int", nullable: false),
                    FeedBack = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Score = table.Column<long>(type: "bigint", nullable: false),
                    ExamPracticeSectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExamPracticeResultId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamPracticeScores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamPracticeScores_ExamPracticeResults_ExamPracticeResultId",
                        column: x => x.ExamPracticeResultId,
                        principalTable: "ExamPracticeResults",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExamPracticeScores_ExamPracticeSections_ExamPracticeSectionId",
                        column: x => x.ExamPracticeSectionId,
                        principalTable: "ExamPracticeSections",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ProsodyScores",
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
                    MinScore = table.Column<double>(type: "float", nullable: false),
                    MaxScore = table.Column<double>(type: "float", nullable: false),
                    BandScore = table.Column<double>(type: "float", nullable: false),
                    BandComment = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProsodyScores", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExamPracticeSectionResults_ExamPracticeSectionId",
                table: "ExamPracticeSectionResults",
                column: "ExamPracticeSectionId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamPracticeAnswers_ExamPracticeSectionId",
                table: "ExamPracticeAnswers",
                column: "ExamPracticeSectionId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamPracticeAnswers_QuestionId",
                table: "ExamPracticeAnswers",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamPracticeScores_ExamPracticeResultId",
                table: "ExamPracticeScores",
                column: "ExamPracticeResultId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamPracticeScores_ExamPracticeSectionId",
                table: "ExamPracticeScores",
                column: "ExamPracticeSectionId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExamPracticeAnswers_ExamPracticeSections_ExamPracticeSectionId",
                table: "ExamPracticeAnswers",
                column: "ExamPracticeSectionId",
                principalTable: "ExamPracticeSections",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ExamPracticeAnswers_Questions_QuestionId",
                table: "ExamPracticeAnswers",
                column: "QuestionId",
                principalTable: "Questions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ExamPracticeSectionResults_ExamPracticeSections_ExamPracticeSectionId",
                table: "ExamPracticeSectionResults",
                column: "ExamPracticeSectionId",
                principalTable: "ExamPracticeSections",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExamPracticeAnswers_ExamPracticeSections_ExamPracticeSectionId",
                table: "ExamPracticeAnswers");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamPracticeAnswers_Questions_QuestionId",
                table: "ExamPracticeAnswers");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamPracticeSectionResults_ExamPracticeSections_ExamPracticeSectionId",
                table: "ExamPracticeSectionResults");

            migrationBuilder.DropTable(
                name: "ExamPracticeScores");

            migrationBuilder.DropTable(
                name: "ProsodyScores");

            migrationBuilder.DropIndex(
                name: "IX_ExamPracticeSectionResults_ExamPracticeSectionId",
                table: "ExamPracticeSectionResults");

            migrationBuilder.DropIndex(
                name: "IX_ExamPracticeAnswers_ExamPracticeSectionId",
                table: "ExamPracticeAnswers");

            migrationBuilder.DropIndex(
                name: "IX_ExamPracticeAnswers_QuestionId",
                table: "ExamPracticeAnswers");

            migrationBuilder.DropColumn(
                name: "CurrentExamPracticeSectionId",
                table: "ExamPracticeSectionResults");

            migrationBuilder.DropColumn(
                name: "ExamPracticeSectionId",
                table: "ExamPracticeSectionResults");

            migrationBuilder.DropColumn(
                name: "WorkingStatus",
                table: "ExamPracticeResults");

            migrationBuilder.DropColumn(
                name: "ExamPracticeSectionId",
                table: "ExamPracticeAnswers");

            migrationBuilder.DropColumn(
                name: "QuestionId",
                table: "ExamPracticeAnswers");

            migrationBuilder.AlterColumn<Guid>(
                name: "ParentExamPracticeSectionResultId",
                table: "ExamPracticeSectionResults",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AnswerStr",
                table: "ExamPracticeAnswers",
                type: "nvarchar(max)",
                maxLength: 11000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldMaxLength: 11000);
        }
    }
}
