using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Fsel.System.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTableQuestBoardConfig_QuestBoardStudent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AchievedPoints",
                table: "QuestBoardStudents",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "DisplayType",
                table: "QuestBoardConfigs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "MaxPoints",
                table: "QuestBoardConfigs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Operator",
                table: "QuestBoardConfigs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TaskPageUrl",
                table: "QuestBoardConfigs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "QuestBoardConfigs",
                keyColumn: "Id",
                keyValue: new Guid("64c0a3a5-849c-412d-86b6-3f5e4809cc84"),
                columns: new[] { "DisplayType", "MaxPoints", "Operator", "TaskPageUrl" },
                values: new object[] { "Number", 1, "Equal", null });

            migrationBuilder.UpdateData(
                table: "QuestBoardConfigs",
                keyColumn: "Id",
                keyValue: new Guid("68bd35d3-c09b-45f3-9a05-0e10691f7c42"),
                columns: new[] { "DisplayType", "MaxPoints", "Operator", "TaskPageUrl" },
                values: new object[] { "Number", 1, "Equal", null });

            migrationBuilder.UpdateData(
                table: "QuestBoardConfigs",
                keyColumn: "Id",
                keyValue: new Guid("72933280-e14b-4715-a802-dcd88e031e79"),
                columns: new[] { "DisplayType", "MaxPoints", "Operator", "TaskPageUrl" },
                values: new object[] { "Number", 1, "Equal", null });

            migrationBuilder.UpdateData(
                table: "QuestBoardConfigs",
                keyColumn: "Id",
                keyValue: new Guid("7b436441-ef2f-4a83-be79-ee5ec5f18355"),
                columns: new[] { "DisplayType", "MaxPoints", "Operator", "TaskPageUrl" },
                values: new object[] { "Number", 1, "Equal", null });

            migrationBuilder.UpdateData(
                table: "QuestBoardConfigs",
                keyColumn: "Id",
                keyValue: new Guid("bb35cdbf-98a7-4ce7-9b71-f457b386d64c"),
                columns: new[] { "DisplayType", "MaxPoints", "Operator", "TaskPageUrl" },
                values: new object[] { "Number", 1, "Equal", null });

            migrationBuilder.UpdateData(
                table: "QuestBoardConfigs",
                keyColumn: "Id",
                keyValue: new Guid("cfce3f4b-66d5-467d-8f57-50a089257bcb"),
                columns: new[] { "DisplayType", "MaxPoints", "Operator", "TaskPageUrl" },
                values: new object[] { "Number", 1, "Equal", null });

            migrationBuilder.InsertData(
                table: "QuestBoardConfigs",
                columns: new[] { "Id", "Category", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "DisplayType", "IsDeleted", "MaxPoints", "Operator", "TaskPageUrl", "Type", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[,]
                {
                    { new Guid("1b35cdbf-98a7-4ce7-9b71-f457b386d62c"), "FinishOneClassForumPost", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Number", false, 1, "Equal", null, "MainQuests", null, null, null },
                    { new Guid("9935cdbf-98a7-4ce7-9b71-f457b386d64c"), "CommentOnOtherPost", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Number", false, 1, "Equal", null, "MainQuests", null, null, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "QuestBoardConfigs",
                keyColumn: "Id",
                keyValue: new Guid("1b35cdbf-98a7-4ce7-9b71-f457b386d62c"));

            migrationBuilder.DeleteData(
                table: "QuestBoardConfigs",
                keyColumn: "Id",
                keyValue: new Guid("9935cdbf-98a7-4ce7-9b71-f457b386d64c"));

            migrationBuilder.DropColumn(
                name: "AchievedPoints",
                table: "QuestBoardStudents");

            migrationBuilder.DropColumn(
                name: "DisplayType",
                table: "QuestBoardConfigs");

            migrationBuilder.DropColumn(
                name: "MaxPoints",
                table: "QuestBoardConfigs");

            migrationBuilder.DropColumn(
                name: "Operator",
                table: "QuestBoardConfigs");

            migrationBuilder.DropColumn(
                name: "TaskPageUrl",
                table: "QuestBoardConfigs");
        }
    }
}
