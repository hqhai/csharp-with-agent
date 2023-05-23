using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateFinalTestResult : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FinalTestExercise_Exercises_ExerciseId",
                table: "FinalTestExercise");

            migrationBuilder.DropForeignKey(
                name: "FK_FinalTestExercise_FinalTests_FinalTestId",
                table: "FinalTestExercise");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FinalTestExercise",
                table: "FinalTestExercise");

            migrationBuilder.RenameTable(
                name: "FinalTestExercise",
                newName: "FinalTestExercises");

            migrationBuilder.RenameIndex(
                name: "IX_FinalTestExercise_FinalTestId",
                table: "FinalTestExercises",
                newName: "IX_FinalTestExercises_FinalTestId");

            migrationBuilder.RenameIndex(
                name: "IX_FinalTestExercise_ExerciseId",
                table: "FinalTestExercises",
                newName: "IX_FinalTestExercises_ExerciseId");

            migrationBuilder.AddColumn<Guid>(
                name: "FinalTestId",
                table: "CourseUnitMockTests",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_FinalTestExercises",
                table: "FinalTestExercises",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "FinalTestResults",
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
                    FinalTestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CourseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinalTestResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinalTestResults_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FinalTestResults_FinalTests_FinalTestId",
                        column: x => x.FinalTestId,
                        principalTable: "FinalTests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FinalTestExerciseAnswers",
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
                    FinalTestResultId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExerciseQuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinalTestExerciseAnswers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinalTestExerciseAnswers_ExerciseQuestions_ExerciseQuestionId",
                        column: x => x.ExerciseQuestionId,
                        principalTable: "ExerciseQuestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FinalTestExerciseAnswers_FinalTestResults_FinalTestResultId",
                        column: x => x.FinalTestResultId,
                        principalTable: "FinalTestResults",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CourseUnitMockTests_FinalTestId",
                table: "CourseUnitMockTests",
                column: "FinalTestId");

            migrationBuilder.CreateIndex(
                name: "IX_FinalTestExerciseAnswers_ExerciseQuestionId",
                table: "FinalTestExerciseAnswers",
                column: "ExerciseQuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_FinalTestExerciseAnswers_FinalTestResultId",
                table: "FinalTestExerciseAnswers",
                column: "FinalTestResultId");

            migrationBuilder.CreateIndex(
                name: "IX_FinalTestResults_CourseId",
                table: "FinalTestResults",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_FinalTestResults_FinalTestId",
                table: "FinalTestResults",
                column: "FinalTestId");

            migrationBuilder.AddForeignKey(
                name: "FK_CourseUnitMockTests_FinalTests_FinalTestId",
                table: "CourseUnitMockTests",
                column: "FinalTestId",
                principalTable: "FinalTests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FinalTestExercises_Exercises_ExerciseId",
                table: "FinalTestExercises",
                column: "ExerciseId",
                principalTable: "Exercises",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FinalTestExercises_FinalTests_FinalTestId",
                table: "FinalTestExercises",
                column: "FinalTestId",
                principalTable: "FinalTests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CourseUnitMockTests_FinalTests_FinalTestId",
                table: "CourseUnitMockTests");

            migrationBuilder.DropForeignKey(
                name: "FK_FinalTestExercises_Exercises_ExerciseId",
                table: "FinalTestExercises");

            migrationBuilder.DropForeignKey(
                name: "FK_FinalTestExercises_FinalTests_FinalTestId",
                table: "FinalTestExercises");

            migrationBuilder.DropTable(
                name: "FinalTestExerciseAnswers");

            migrationBuilder.DropTable(
                name: "FinalTestResults");

            migrationBuilder.DropIndex(
                name: "IX_CourseUnitMockTests_FinalTestId",
                table: "CourseUnitMockTests");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FinalTestExercises",
                table: "FinalTestExercises");

            migrationBuilder.DropColumn(
                name: "FinalTestId",
                table: "CourseUnitMockTests");

            migrationBuilder.RenameTable(
                name: "FinalTestExercises",
                newName: "FinalTestExercise");

            migrationBuilder.RenameIndex(
                name: "IX_FinalTestExercises_FinalTestId",
                table: "FinalTestExercise",
                newName: "IX_FinalTestExercise_FinalTestId");

            migrationBuilder.RenameIndex(
                name: "IX_FinalTestExercises_ExerciseId",
                table: "FinalTestExercise",
                newName: "IX_FinalTestExercise_ExerciseId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FinalTestExercise",
                table: "FinalTestExercise",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FinalTestExercise_Exercises_ExerciseId",
                table: "FinalTestExercise",
                column: "ExerciseId",
                principalTable: "Exercises",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FinalTestExercise_FinalTests_FinalTestId",
                table: "FinalTestExercise",
                column: "FinalTestId",
                principalTable: "FinalTests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
