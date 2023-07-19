using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateExtraPracticeResult : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "SectionQuestionId",
                table: "MockTestAnswers",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<Guid>(
                name: "SectionId",
                table: "MockTestAnswers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SectionTimeCodeId",
                table: "MockTestAnswers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "VideoLink",
                table: "ExtraPractices",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BookFilePath",
                table: "ExtraPractices",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BookCoverPath",
                table: "ExtraPractices",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BookBackgroundPath",
                table: "ExtraPractices",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Author",
                table: "ExtraPractices",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "ExtraPracticeResults",
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
                    CorrectCount = table.Column<int>(type: "int", nullable: false),
                    CorrectTotal = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SkillScoresStr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExtraPracticeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExtraPracticeResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExtraPracticeResults_ExtraPractices_ExtraPracticeId",
                        column: x => x.ExtraPracticeId,
                        principalTable: "ExtraPractices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExtraPracticeExerciseResults",
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
                    CorrectCount = table.Column<int>(type: "int", nullable: false),
                    ExecuteCount = table.Column<int>(type: "int", nullable: false),
                    CorrectTotal = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CourseSkill = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExtraPracticeResultId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExtraPracticeExerciseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExtraPracticeExerciseResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExtraPracticeExerciseResults_ExtraPracticeExercises_ExtraPracticeExerciseId",
                        column: x => x.ExtraPracticeExerciseId,
                        principalTable: "ExtraPracticeExercises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExtraPracticeExerciseResults_ExtraPracticeResults_ExtraPracticeResultId",
                        column: x => x.ExtraPracticeResultId,
                        principalTable: "ExtraPracticeResults",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ExtraPracticeAnswers",
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
                    AnswerStr = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CorrectCount = table.Column<int>(type: "int", nullable: false),
                    ExtraPracticeResultId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ExtraPracticeExerciseResultId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SectionTimeCodeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    QuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExtraPracticeAnswers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExtraPracticeAnswers_ExtraPracticeExerciseResults_ExtraPracticeExerciseResultId",
                        column: x => x.ExtraPracticeExerciseResultId,
                        principalTable: "ExtraPracticeExerciseResults",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExtraPracticeAnswers_ExtraPracticeResults_ExtraPracticeResultId",
                        column: x => x.ExtraPracticeResultId,
                        principalTable: "ExtraPracticeResults",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExtraPracticeAnswers_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExtraPracticeAnswers_SectionTimeCodes_SectionTimeCodeId",
                        column: x => x.SectionTimeCodeId,
                        principalTable: "SectionTimeCodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExtraPracticeAnswers_Sections_SectionId",
                        column: x => x.SectionId,
                        principalTable: "Sections",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_MockTestAnswers_SectionId",
                table: "MockTestAnswers",
                column: "SectionId");

            migrationBuilder.CreateIndex(
                name: "IX_MockTestAnswers_SectionTimeCodeId",
                table: "MockTestAnswers",
                column: "SectionTimeCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_ExtraPracticeAnswers_ExtraPracticeExerciseResultId",
                table: "ExtraPracticeAnswers",
                column: "ExtraPracticeExerciseResultId");

            migrationBuilder.CreateIndex(
                name: "IX_ExtraPracticeAnswers_ExtraPracticeResultId",
                table: "ExtraPracticeAnswers",
                column: "ExtraPracticeResultId");

            migrationBuilder.CreateIndex(
                name: "IX_ExtraPracticeAnswers_QuestionId",
                table: "ExtraPracticeAnswers",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_ExtraPracticeAnswers_SectionId",
                table: "ExtraPracticeAnswers",
                column: "SectionId");

            migrationBuilder.CreateIndex(
                name: "IX_ExtraPracticeAnswers_SectionTimeCodeId",
                table: "ExtraPracticeAnswers",
                column: "SectionTimeCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_ExtraPracticeExerciseResults_ExtraPracticeExerciseId",
                table: "ExtraPracticeExerciseResults",
                column: "ExtraPracticeExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_ExtraPracticeExerciseResults_ExtraPracticeResultId",
                table: "ExtraPracticeExerciseResults",
                column: "ExtraPracticeResultId");

            migrationBuilder.CreateIndex(
                name: "IX_ExtraPracticeResults_ExtraPracticeId",
                table: "ExtraPracticeResults",
                column: "ExtraPracticeId");

            migrationBuilder.AddForeignKey(
                name: "FK_MockTestAnswers_SectionTimeCodes_SectionTimeCodeId",
                table: "MockTestAnswers",
                column: "SectionTimeCodeId",
                principalTable: "SectionTimeCodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MockTestAnswers_Sections_SectionId",
                table: "MockTestAnswers",
                column: "SectionId",
                principalTable: "Sections",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MockTestAnswers_SectionTimeCodes_SectionTimeCodeId",
                table: "MockTestAnswers");

            migrationBuilder.DropForeignKey(
                name: "FK_MockTestAnswers_Sections_SectionId",
                table: "MockTestAnswers");

            migrationBuilder.DropTable(
                name: "ExtraPracticeAnswers");

            migrationBuilder.DropTable(
                name: "ExtraPracticeExerciseResults");

            migrationBuilder.DropTable(
                name: "ExtraPracticeResults");

            migrationBuilder.DropIndex(
                name: "IX_MockTestAnswers_SectionId",
                table: "MockTestAnswers");

            migrationBuilder.DropIndex(
                name: "IX_MockTestAnswers_SectionTimeCodeId",
                table: "MockTestAnswers");

            migrationBuilder.DropColumn(
                name: "SectionId",
                table: "MockTestAnswers");

            migrationBuilder.DropColumn(
                name: "SectionTimeCodeId",
                table: "MockTestAnswers");

            migrationBuilder.AlterColumn<Guid>(
                name: "SectionQuestionId",
                table: "MockTestAnswers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "VideoLink",
                table: "ExtraPractices",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BookFilePath",
                table: "ExtraPractices",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BookCoverPath",
                table: "ExtraPractices",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BookBackgroundPath",
                table: "ExtraPractices",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Author",
                table: "ExtraPractices",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);
        }
    }
}
