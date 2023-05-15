using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreatePlacementTestSectionAnswer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlacementTestSectionResults",
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
                    EndTime = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    PlacementTestSectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SectionGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlacementTestSectionResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlacementTestSectionResults_PlacementTestSections_PlacementTestSectionId",
                        column: x => x.PlacementTestSectionId,
                        principalTable: "PlacementTestSections",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PlacementTestSectionResults_SectionGroups_SectionGroupId",
                        column: x => x.SectionGroupId,
                        principalTable: "SectionGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlacementTestAnswers",
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
                    PlacementTestSectionResultId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SectionQuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlacementTestAnswers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlacementTestAnswers_PlacementTestSectionResults_PlacementTestSectionResultId",
                        column: x => x.PlacementTestSectionResultId,
                        principalTable: "PlacementTestSectionResults",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlacementTestAnswers_SectionQuestions_SectionQuestionId",
                        column: x => x.SectionQuestionId,
                        principalTable: "SectionQuestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlacementTestAnswers_PlacementTestSectionResultId",
                table: "PlacementTestAnswers",
                column: "PlacementTestSectionResultId");

            migrationBuilder.CreateIndex(
                name: "IX_PlacementTestAnswers_SectionQuestionId",
                table: "PlacementTestAnswers",
                column: "SectionQuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_PlacementTestSectionResults_PlacementTestSectionId",
                table: "PlacementTestSectionResults",
                column: "PlacementTestSectionId");

            migrationBuilder.CreateIndex(
                name: "IX_PlacementTestSectionResults_SectionGroupId",
                table: "PlacementTestSectionResults",
                column: "SectionGroupId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlacementTestAnswers");

            migrationBuilder.DropTable(
                name: "PlacementTestSectionResults");
        }
    }
}
