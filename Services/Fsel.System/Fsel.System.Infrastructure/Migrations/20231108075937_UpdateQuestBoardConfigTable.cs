using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Fsel.System.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateQuestBoardConfigTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<float>(
                name: "AchievedPoints",
                table: "QuestBoardStudents",
                type: "real",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.InsertData(
                table: "QuestBoardConfigs",
                columns: new[] { "Id", "Category", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "DisplayType", "IsDeleted", "MaxPoints", "Operator", "TaskPageUrl", "Type", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[,]
                {
                    { new Guid("0407d174-a779-46e9-bbe4-5f27ae6075b6"), "ParticipationScore", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Percent", false, 100, "Equal", null, "PremiumQuests", null, null, null },
                    { new Guid("0c1633d5-f145-44c7-80c3-a167c0bbd1a1"), "PostThreeDiscussionBoard", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Number", false, 3, "Equal", null, "PremiumQuests", null, null, null },
                    { new Guid("7a37fa61-1a04-4d70-8590-90fa3d563d8e"), "SeeAllTeacherReview", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Percent", false, 100, "Equal", null, "PremiumQuests", null, null, null },
                    { new Guid("8333c5e5-e853-45fb-8abf-1a09336af78e"), "SeeFiveTeacherReview", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Number", false, 5, "Equal", null, "PremiumQuests", null, null, null },
                    { new Guid("85659470-7d45-4ddf-8b3c-dba6458bf4f4"), "PostFiveDiscussionBoard", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Number", false, 5, "Equal", null, "PremiumQuests", null, null, null },
                    { new Guid("ac0f0c74-cc2a-41a2-a82f-748ed5f2c75c"), "SeeTenTeacherReview", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Number", false, 10, "Equal", null, "PremiumQuests", null, null, null },
                    { new Guid("f31d44ff-226e-4244-a9a0-a06ed397368a"), "PostOneDiscussionBoard", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Number", false, 1, "Equal", null, "PremiumQuests", null, null, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "QuestBoardConfigs",
                keyColumn: "Id",
                keyValue: new Guid("0407d174-a779-46e9-bbe4-5f27ae6075b6"));

            migrationBuilder.DeleteData(
                table: "QuestBoardConfigs",
                keyColumn: "Id",
                keyValue: new Guid("0c1633d5-f145-44c7-80c3-a167c0bbd1a1"));

            migrationBuilder.DeleteData(
                table: "QuestBoardConfigs",
                keyColumn: "Id",
                keyValue: new Guid("7a37fa61-1a04-4d70-8590-90fa3d563d8e"));

            migrationBuilder.DeleteData(
                table: "QuestBoardConfigs",
                keyColumn: "Id",
                keyValue: new Guid("8333c5e5-e853-45fb-8abf-1a09336af78e"));

            migrationBuilder.DeleteData(
                table: "QuestBoardConfigs",
                keyColumn: "Id",
                keyValue: new Guid("85659470-7d45-4ddf-8b3c-dba6458bf4f4"));

            migrationBuilder.DeleteData(
                table: "QuestBoardConfigs",
                keyColumn: "Id",
                keyValue: new Guid("ac0f0c74-cc2a-41a2-a82f-748ed5f2c75c"));

            migrationBuilder.DeleteData(
                table: "QuestBoardConfigs",
                keyColumn: "Id",
                keyValue: new Guid("f31d44ff-226e-4244-a9a0-a06ed397368a"));

            migrationBuilder.AlterColumn<int>(
                name: "AchievedPoints",
                table: "QuestBoardStudents",
                type: "int",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "real");
        }
    }
}
