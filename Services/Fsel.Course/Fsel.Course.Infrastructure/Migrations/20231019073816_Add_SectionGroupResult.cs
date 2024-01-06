using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_SectionGroupResult : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SectionGroupResults",
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
                    SkillScoresStr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SectionGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MockTestResultId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FinalTestResultId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ExtraPracticeResultId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SectionGroupResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SectionGroupResults_ExtraPracticeResults_ExtraPracticeResultId",
                        column: x => x.ExtraPracticeResultId,
                        principalTable: "ExtraPracticeResults",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SectionGroupResults_FinalTestResults_FinalTestResultId",
                        column: x => x.FinalTestResultId,
                        principalTable: "FinalTestResults",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SectionGroupResults_MockTestResults_MockTestResultId",
                        column: x => x.MockTestResultId,
                        principalTable: "MockTestResults",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SectionGroupResults_SectionGroups_SectionGroupId",
                        column: x => x.SectionGroupId,
                        principalTable: "SectionGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SectionGroupResults_ExtraPracticeResultId",
                table: "SectionGroupResults",
                column: "ExtraPracticeResultId");

            migrationBuilder.CreateIndex(
                name: "IX_SectionGroupResults_FinalTestResultId",
                table: "SectionGroupResults",
                column: "FinalTestResultId");

            migrationBuilder.CreateIndex(
                name: "IX_SectionGroupResults_MockTestResultId",
                table: "SectionGroupResults",
                column: "MockTestResultId");

            migrationBuilder.CreateIndex(
                name: "IX_SectionGroupResults_SectionGroupId",
                table: "SectionGroupResults",
                column: "SectionGroupId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SectionGroupResults");
        }
    }
}
