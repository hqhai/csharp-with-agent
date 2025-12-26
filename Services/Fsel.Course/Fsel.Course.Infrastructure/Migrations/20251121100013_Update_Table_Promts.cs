using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_Table_Promts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AiPromptManagers_AiPromptManagers_ParentId",
                table: "AiPromptManagers");

            migrationBuilder.DropIndex(
                name: "IX_AiPromptManagers_ParentId",
                table: "AiPromptManagers");

            migrationBuilder.DropColumn(
                name: "FeatureAi",
                table: "AiPromptManagers");

            migrationBuilder.RenameColumn(
                name: "ParentId",
                table: "AiPromptManagers",
                newName: "ProjectId");

            migrationBuilder.AddColumn<Guid>(
                name: "AiPromptParentId",
                table: "AiPromptManagers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TypeCriteriaAi",
                table: "AICriteriaConfigs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<string>(
                name: "FeatureMultiple",
                table: "AICriteriaConfigs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ObjectId",
                table: "AICriteriaConfigs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProjectId",
                table: "AICriteriaConfigs",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "SubFeatureType",
                table: "AICriteriaConfigs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AiPromptManagers_AiPromptParentId",
                table: "AiPromptManagers",
                column: "AiPromptParentId");

            migrationBuilder.AddForeignKey(
                name: "FK_AiPromptManagers_AiPromptManagers_AiPromptParentId",
                table: "AiPromptManagers",
                column: "AiPromptParentId",
                principalTable: "AiPromptManagers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AiPromptManagers_AiPromptManagers_AiPromptParentId",
                table: "AiPromptManagers");

            migrationBuilder.DropIndex(
                name: "IX_AiPromptManagers_AiPromptParentId",
                table: "AiPromptManagers");

            migrationBuilder.DropColumn(
                name: "AiPromptParentId",
                table: "AiPromptManagers");

            migrationBuilder.DropColumn(
                name: "FeatureMultiple",
                table: "AICriteriaConfigs");

            migrationBuilder.DropColumn(
                name: "ObjectId",
                table: "AICriteriaConfigs");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "AICriteriaConfigs");

            migrationBuilder.DropColumn(
                name: "SubFeatureType",
                table: "AICriteriaConfigs");

            migrationBuilder.RenameColumn(
                name: "ProjectId",
                table: "AiPromptManagers",
                newName: "ParentId");

            migrationBuilder.AddColumn<string>(
                name: "FeatureAi",
                table: "AiPromptManagers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "TypeCriteriaAi",
                table: "AICriteriaConfigs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AiPromptManagers_ParentId",
                table: "AiPromptManagers",
                column: "ParentId");

            migrationBuilder.AddForeignKey(
                name: "FK_AiPromptManagers_AiPromptManagers_ParentId",
                table: "AiPromptManagers",
                column: "ParentId",
                principalTable: "AiPromptManagers",
                principalColumn: "Id");
        }
    }
}
