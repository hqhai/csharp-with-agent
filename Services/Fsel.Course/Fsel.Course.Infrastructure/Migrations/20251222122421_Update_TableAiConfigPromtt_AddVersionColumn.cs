using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_TableAiConfigPromtt_AddVersionColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AiPromptManagers_AiPromptManagers_AiPromptParentId",
                table: "AiPromptManagers");


            migrationBuilder.RenameColumn(
                name: "AiPromptParentId",
                table: "AiPromptManagers",
                newName: "OriginalId");

            migrationBuilder.AddColumn<Guid>(
                name: "AiPromptManagerParentId",
                table: "AiPromptManagers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "AiPromptManagers",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<string>(
                name: "VersionStatus",
                table: "AiPromptManagers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "VersionType",
                table: "AiPromptManagers",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "V2");

            migrationBuilder.AddColumn<Guid>(
                name: "OriginalId",
                table: "AICriteriaConfigs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "AICriteriaConfigs",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<string>(
                name: "VersionStatus",
                table: "AICriteriaConfigs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "VersionType",
                table: "AICriteriaConfigs",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_AiPromptManagers_AiPromptManagers_AiPromptManagerParentId",
                table: "AiPromptManagers",
                column: "AiPromptManagerParentId",
                principalTable: "AiPromptManagers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AiPromptManagers_AiPromptManagers_AiPromptManagerParentId",
                table: "AiPromptManagers");

            migrationBuilder.DropColumn(
                name: "AiPromptManagerParentId",
                table: "AiPromptManagers");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "AiPromptManagers");

            migrationBuilder.DropColumn(
                name: "VersionStatus",
                table: "AiPromptManagers");

            migrationBuilder.DropColumn(
                name: "VersionType",
                table: "AiPromptManagers");

            migrationBuilder.DropColumn(
                name: "OriginalId",
                table: "AICriteriaConfigs");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "AICriteriaConfigs");

            migrationBuilder.DropColumn(
                name: "VersionStatus",
                table: "AICriteriaConfigs");

            migrationBuilder.DropColumn(
                name: "VersionType",
                table: "AICriteriaConfigs");

            migrationBuilder.RenameColumn(
                name: "OriginalId",
                table: "AiPromptManagers",
                newName: "AiPromptParentId");
        }
    }
}
