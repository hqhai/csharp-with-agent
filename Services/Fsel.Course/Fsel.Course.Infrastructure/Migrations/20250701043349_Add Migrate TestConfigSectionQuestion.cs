using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMigrateTestConfigSectionQuestion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TestConfigSectionQuestions",
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
                    QuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TestConfigSectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestConfigSectionQuestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TestConfigSectionQuestions_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TestConfigSectionQuestions_TestConfigSections_TestConfigSectionId",
                        column: x => x.TestConfigSectionId,
                        principalTable: "TestConfigSections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TestConfigSections_ParentId",
                table: "TestConfigSections",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_TestConfigSectionQuestions_QuestionId",
                table: "TestConfigSectionQuestions",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_TestConfigSectionQuestions_TestConfigSectionId",
                table: "TestConfigSectionQuestions",
                column: "TestConfigSectionId");

            migrationBuilder.AddForeignKey(
                name: "FK_TestConfigSections_TestConfigSections_ParentId",
                table: "TestConfigSections",
                column: "ParentId",
                principalTable: "TestConfigSections",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TestConfigSections_TestConfigSections_ParentId",
                table: "TestConfigSections");

            migrationBuilder.DropTable(
                name: "TestConfigSectionQuestions");

            migrationBuilder.DropIndex(
                name: "IX_TestConfigSections_ParentId",
                table: "TestConfigSections");
        }
    }
}
