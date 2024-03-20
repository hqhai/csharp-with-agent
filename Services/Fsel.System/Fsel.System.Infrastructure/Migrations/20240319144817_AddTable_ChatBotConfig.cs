using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.System.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTable_ChatBotConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChatbotConfigs",
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
                    ProgramName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    CourseName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    CEFRLevel = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    UnitNumber = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    NumberSkill = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatbotConfigs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChatbotSkillConfigs",
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
                    Skill = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Config = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AiConfig = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ChatbotConfigId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatbotSkillConfigs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatbotSkillConfigs_ChatbotConfigs_ChatbotConfigId",
                        column: x => x.ChatbotConfigId,
                        principalTable: "ChatbotConfigs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChatbotTokenConfigs",
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
                    ReadingToken = table.Column<int>(type: "int", nullable: false),
                    ListeningToken = table.Column<int>(type: "int", nullable: false),
                    WritingToken = table.Column<int>(type: "int", nullable: false),
                    SpeakingToken = table.Column<int>(type: "int", nullable: false),
                    VocabularyToken = table.Column<int>(type: "int", nullable: false),
                    GrammarToken = table.Column<int>(type: "int", nullable: false),
                    ChatbotConfigId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatbotTokenConfigs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatbotTokenConfigs_ChatbotConfigs_ChatbotConfigId",
                        column: x => x.ChatbotConfigId,
                        principalTable: "ChatbotConfigs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChatbotSkillConfigs_ChatbotConfigId",
                table: "ChatbotSkillConfigs",
                column: "ChatbotConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatbotTokenConfigs_ChatbotConfigId",
                table: "ChatbotTokenConfigs",
                column: "ChatbotConfigId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChatbotSkillConfigs");

            migrationBuilder.DropTable(
                name: "ChatbotTokenConfigs");

            migrationBuilder.DropTable(
                name: "ChatbotConfigs");
        }
    }
}
