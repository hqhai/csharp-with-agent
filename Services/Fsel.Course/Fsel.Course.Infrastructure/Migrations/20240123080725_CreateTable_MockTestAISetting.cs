using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateTable_MockTestAISetting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
               name: "GradingAlFeedback",
               table: "MockTestAnswers",
               type: "nvarchar(max)",
               nullable: true);

            migrationBuilder.CreateTable(
                name: "MockTestAISettings",
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
                    SectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Task = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PromptStr = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MockTestAISettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MockTestAISettings_Sections_SectionId",
                        column: x => x.SectionId,
                        principalTable: "Sections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MockTestAISettings_SectionId",
                table: "MockTestAISettings",
                column: "SectionId",
                unique: true,
                filter: "[SectionId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MockTestAISettings");

            migrationBuilder.DropColumn(
               name: "GradingAlFeedback",
               table: "MockTestAnswers");
        }
    }
}
