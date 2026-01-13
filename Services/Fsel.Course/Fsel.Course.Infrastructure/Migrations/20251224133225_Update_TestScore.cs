using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_TestScore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TestAnswer_TestResult_TestResultId",
                table: "TestAnswer");

            migrationBuilder.AddColumn<Guid>(
                name: "CurrentSectionTimeCodeId",
                table: "TestSectionResult",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "PercentModule",
                table: "TestSectionResult",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Score",
                table: "TestResult",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TestSectionId",
                table: "TestAnswer",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TestScores",
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
                    Criteria = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Feedback = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Score = table.Column<double>(type: "float", nullable: false),
                    TestResultId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TestSectionResultId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TestSectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestScores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TestScores_TestResult_TestResultId",
                        column: x => x.TestResultId,
                        principalTable: "TestResult",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TestScores_TestSectionResult_TestSectionResultId",
                        column: x => x.TestSectionResultId,
                        principalTable: "TestSectionResult",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TestScores_TestSections_TestSectionId",
                        column: x => x.TestSectionId,
                        principalTable: "TestSections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TestAnswer_TestSectionId",
                table: "TestAnswer",
                column: "TestSectionId");

            migrationBuilder.CreateIndex(
                name: "IX_TestScores_TestResultId",
                table: "TestScores",
                column: "TestResultId");

            migrationBuilder.CreateIndex(
                name: "IX_TestScores_TestSectionId",
                table: "TestScores",
                column: "TestSectionId");

            migrationBuilder.CreateIndex(
                name: "IX_TestScores_TestSectionResultId",
                table: "TestScores",
                column: "TestSectionResultId");

            migrationBuilder.AddForeignKey(
                name: "FK_TestAnswer_TestResult_TestResultId",
                table: "TestAnswer",
                column: "TestResultId",
                principalTable: "TestResult",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TestAnswer_TestSections_TestSectionId",
                table: "TestAnswer",
                column: "TestSectionId",
                principalTable: "TestSections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TestAnswer_TestResult_TestResultId",
                table: "TestAnswer");

            migrationBuilder.DropForeignKey(
                name: "FK_TestAnswer_TestSections_TestSectionId",
                table: "TestAnswer");

            migrationBuilder.DropTable(
                name: "TestScores");

            migrationBuilder.DropIndex(
                name: "IX_TestAnswer_TestSectionId",
                table: "TestAnswer");

            migrationBuilder.DropColumn(
                name: "CurrentSectionTimeCodeId",
                table: "TestSectionResult");

            migrationBuilder.DropColumn(
                name: "PercentModule",
                table: "TestSectionResult");

            migrationBuilder.DropColumn(
                name: "Score",
                table: "TestResult");

            migrationBuilder.DropColumn(
                name: "TestSectionId",
                table: "TestAnswer");

            migrationBuilder.AddForeignKey(
                name: "FK_TestAnswer_TestResult_TestResultId",
                table: "TestAnswer",
                column: "TestResultId",
                principalTable: "TestResult",
                principalColumn: "Id");
        }
    }
}
