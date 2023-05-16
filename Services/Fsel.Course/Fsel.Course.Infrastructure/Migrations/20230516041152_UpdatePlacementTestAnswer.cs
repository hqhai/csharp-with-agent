using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePlacementTestAnswer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlacementTestAnswers_PlacementTestSectionResults_PlacementTestSectionResultId",
                table: "PlacementTestAnswers");

            migrationBuilder.DropTable(
                name: "PlacementTestSectionResults");

            migrationBuilder.RenameColumn(
                name: "PlacementTestSectionResultId",
                table: "PlacementTestAnswers",
                newName: "PlacementTestResultId");

            migrationBuilder.RenameIndex(
                name: "IX_PlacementTestAnswers_PlacementTestSectionResultId",
                table: "PlacementTestAnswers",
                newName: "IX_PlacementTestAnswers_PlacementTestResultId");

            migrationBuilder.CreateTable(
                name: "PlacementTestResults",
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
                    Status = table.Column<int>(type: "int", nullable: false),
                    PlacementTestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlacementTestResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlacementTestResults_PlacementTests_PlacementTestId",
                        column: x => x.PlacementTestId,
                        principalTable: "PlacementTests",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlacementTestResults_PlacementTestId",
                table: "PlacementTestResults",
                column: "PlacementTestId");

            migrationBuilder.AddForeignKey(
                name: "FK_PlacementTestAnswers_PlacementTestResults_PlacementTestResultId",
                table: "PlacementTestAnswers",
                column: "PlacementTestResultId",
                principalTable: "PlacementTestResults",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlacementTestAnswers_PlacementTestResults_PlacementTestResultId",
                table: "PlacementTestAnswers");

            migrationBuilder.DropTable(
                name: "PlacementTestResults");

            migrationBuilder.RenameColumn(
                name: "PlacementTestResultId",
                table: "PlacementTestAnswers",
                newName: "PlacementTestSectionResultId");

            migrationBuilder.RenameIndex(
                name: "IX_PlacementTestAnswers_PlacementTestResultId",
                table: "PlacementTestAnswers",
                newName: "IX_PlacementTestAnswers_PlacementTestSectionResultId");

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
                    PlacementTestSectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CorrectCount = table.Column<int>(type: "int", nullable: false),
                    CorrectTotal = table.Column<int>(type: "int", nullable: false),
                    EndTime = table.Column<long>(type: "bigint", nullable: false),
                    Percent = table.Column<double>(type: "float", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlacementTestSectionResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlacementTestSectionResults_PlacementTestSections_PlacementTestSectionId",
                        column: x => x.PlacementTestSectionId,
                        principalTable: "PlacementTestSections",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlacementTestSectionResults_PlacementTestSectionId",
                table: "PlacementTestSectionResults",
                column: "PlacementTestSectionId");

            migrationBuilder.AddForeignKey(
                name: "FK_PlacementTestAnswers_PlacementTestSectionResults_PlacementTestSectionResultId",
                table: "PlacementTestAnswers",
                column: "PlacementTestSectionResultId",
                principalTable: "PlacementTestSectionResults",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
