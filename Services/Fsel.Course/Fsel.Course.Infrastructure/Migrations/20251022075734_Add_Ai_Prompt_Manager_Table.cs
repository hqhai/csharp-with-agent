using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_Ai_Prompt_Manager_Table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AiPromptManagers",
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
                    AiModelName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    InputModel = table.Column<string>(type: "nvarchar(max)", maxLength: 100000, nullable: false),
                    FeatureAi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FeatureObjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiPromptManagers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AiPromptManagers_AiPromptManagers_ParentId",
                        column: x => x.ParentId,
                        principalTable: "AiPromptManagers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AICriteriaConfigs",
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
                    AiPromptManagerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TypeCriteriaAi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UserRole = table.Column<string>(type: "nvarchar(max)", maxLength: 100000, nullable: true),
                    SettingAiConfig = table.Column<string>(type: "nvarchar(max)", maxLength: 100000, nullable: true),
                    SettingAiJson = table.Column<string>(type: "nvarchar(max)", maxLength: 100000, nullable: true),
                    SettingTemperature = table.Column<double>(type: "float", nullable: true),
                    SettingWordMaxLength = table.Column<double>(type: "float", nullable: true),
                    SettingTopP = table.Column<double>(type: "float", nullable: true),
                    SettingFrequency = table.Column<double>(type: "float", nullable: true),
                    SettingPresence = table.Column<double>(type: "float", nullable: true),
                    MaximumNumber = table.Column<int>(type: "int", nullable: true),
                    MaximumToken = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AICriteriaConfigs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AICriteriaConfigs_AiPromptManagers_AiPromptManagerId",
                        column: x => x.AiPromptManagerId,
                        principalTable: "AiPromptManagers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AICriteriaConfigs_AiPromptManagerId",
                table: "AICriteriaConfigs",
                column: "AiPromptManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_AiPromptManagers_ParentId",
                table: "AiPromptManagers",
                column: "ParentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AICriteriaConfigs");

            migrationBuilder.DropTable(
                name: "AiPromptManagers");
        }
    }
}
