using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.System.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_Chatbot_New_Core : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AICriteriaConfigId",
                table: "ChatbotSkillConfigs",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "ChatbotLayout",
                table: "ChatbotSkillConfigs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SkillFilePath",
                table: "ChatbotSkillConfigs",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SkillId",
                table: "ChatbotSkillConfigs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SkillName",
                table: "ChatbotSkillConfigs",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Token",
                table: "ChatbotSkillConfigs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "SkillName",
                table: "ChatBots",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SkillFilePath",
                table: "ChatBots",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProgramId",
                table: "ChatbotConfigs",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AICriteriaConfigId",
                table: "ChatbotSkillConfigs");

            migrationBuilder.DropColumn(
                name: "ChatbotLayout",
                table: "ChatbotSkillConfigs");

            migrationBuilder.DropColumn(
                name: "SkillFilePath",
                table: "ChatbotSkillConfigs");

            migrationBuilder.DropColumn(
                name: "SkillId",
                table: "ChatbotSkillConfigs");

            migrationBuilder.DropColumn(
                name: "SkillName",
                table: "ChatbotSkillConfigs");

            migrationBuilder.DropColumn(
                name: "Token",
                table: "ChatbotSkillConfigs");

            migrationBuilder.DropColumn(
                name: "SkillFilePath",
                table: "ChatBots");

            migrationBuilder.DropColumn(
                name: "ProgramId",
                table: "ChatbotConfigs");

            migrationBuilder.AlterColumn<string>(
                name: "SkillName",
                table: "ChatBots",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 250,
                oldNullable: true);
        }
    }
}
