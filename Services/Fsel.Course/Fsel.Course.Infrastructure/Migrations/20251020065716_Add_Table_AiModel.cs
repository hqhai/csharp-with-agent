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
                name: "AiModelManager",
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
                    table.PrimaryKey("PK_AiModelManager", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AiModelFeature",
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
                    AiModelManagerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FeatureObjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParentFeatureId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FeatureAi = table.Column<int>(type: "int", nullable: false),
                    TypeFeatureAi = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_AiModelFeature", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AiModelFeature_AiModelFeature_ParentFeatureId",
                        column: x => x.ParentFeatureId,
                        principalTable: "AiModelFeature",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AiModelFeature_AiModelManager_AiModelManagerId",
                        column: x => x.AiModelManagerId,
                        principalTable: "AiModelManager",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AiModelFeature_AiModelManagerId",
                table: "AiModelFeature",
                column: "AiModelManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_AiModelFeature_ParentFeatureId",
                table: "AiModelFeature",
                column: "ParentFeatureId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AiModelFeature");

            migrationBuilder.DropTable(
                name: "AiModelManager");
        }
    }
}
