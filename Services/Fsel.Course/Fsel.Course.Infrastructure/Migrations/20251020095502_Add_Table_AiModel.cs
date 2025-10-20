using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_Table_AiModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AiPromptManager",
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
                    InputModel = table.Column<string>(type: "nvarchar(max)", maxLength: 100000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiPromptManager", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AIFeatureConfig",
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
                    FeatureObjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParentFeatureId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FeatureAi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TypeFeatureAi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UserRole = table.Column<string>(type: "nvarchar(max)", maxLength: 100000, nullable: true),
                    Config = table.Column<string>(type: "nvarchar(max)", maxLength: 100000, nullable: true),
                    Json = table.Column<string>(type: "nvarchar(max)", maxLength: 100000, nullable: true),
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
                    table.PrimaryKey("PK_AIFeatureConfig", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AIFeatureConfig_AIFeatureConfig_ParentFeatureId",
                        column: x => x.ParentFeatureId,
                        principalTable: "AIFeatureConfig",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AIFeatureConfig_AiPromptManager_AiPromptManagerId",
                        column: x => x.AiPromptManagerId,
                        principalTable: "AiPromptManager",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AIFeatureConfig_AiPromptManagerId",
                table: "AIFeatureConfig",
                column: "AiPromptManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_AIFeatureConfig_ParentFeatureId",
                table: "AIFeatureConfig",
                column: "ParentFeatureId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AIFeatureConfig");

            migrationBuilder.DropTable(
                name: "AiPromptManager");
        }
    }
}
