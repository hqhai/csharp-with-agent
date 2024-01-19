using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateColumn_RenameTable_MockTestAISetting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AiGradeSettings");

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
                    ObjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Task = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PromptStr = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MockTestAISettings", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MockTestAISettings");

            migrationBuilder.CreateTable(
                name: "AiGradeSettings",
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
                    ObjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SettingFrequecy = table.Column<double>(type: "float", nullable: false),
                    SettingModel = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    SettingPresence = table.Column<double>(type: "float", nullable: false),
                    SettingTemperature = table.Column<double>(type: "float", nullable: false),
                    SettingTopP = table.Column<double>(type: "float", nullable: false),
                    SettingWordMaxLength = table.Column<double>(type: "float", nullable: false),
                    SystemRoleAlConfig = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserAlConfig = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiGradeSettings", x => x.Id);
                });
        }
    }
}
