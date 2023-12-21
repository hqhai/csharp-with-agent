using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Fsel.System.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateQuestBoardConfigTable_V2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "QuestBoardConfigs",
                keyColumn: "Id",
                keyValue: new Guid("0a20cb15-8629-41d9-88d9-890a3b487496"));

            migrationBuilder.DeleteData(
                table: "QuestBoardConfigs",
                keyColumn: "Id",
                keyValue: new Guid("0ddf9f6f-d2cb-49c2-858a-f4afb3233dee"));

            migrationBuilder.DeleteData(
                table: "QuestBoardConfigs",
                keyColumn: "Id",
                keyValue: new Guid("2ca29e0c-4f81-40c6-9a0c-040d270d0433"));

            migrationBuilder.DeleteData(
                table: "QuestBoardConfigs",
                keyColumn: "Id",
                keyValue: new Guid("406c775b-51e8-4742-8c4b-a898ae2b8924"));

            migrationBuilder.DeleteData(
                table: "QuestBoardConfigs",
                keyColumn: "Id",
                keyValue: new Guid("62767c8d-6e18-4d11-a74e-fe38eab7fa58"));

            migrationBuilder.DeleteData(
                table: "QuestBoardConfigs",
                keyColumn: "Id",
                keyValue: new Guid("e1c02bbc-0c98-40a0-b7e0-807b5f137c3b"));

            migrationBuilder.InsertData(
                table: "QuestBoardConfigs",
                columns: new[] { "Id", "Category", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "DisplayType", "IsDeleted", "MaxPoints", "Operator", "TaskPageUrl", "Type", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[,]
                {
                    { new Guid("0417a848-1cd0-4aef-ae33-2757652701d0"), "RateAndComment", new DateTime(2023, 11, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Number", false, 1, "Equal", null, "SideQuests", null, null, null },
                    { new Guid("0aee60ff-65bc-4277-803c-8a21c38b2b84"), "ThirtyMinutesFocusMode", new DateTime(2023, 11, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Number", false, 1, "Equal", null, "SideQuests", null, null, null },
                    { new Guid("c3c2c8aa-fc4a-4b01-8d55-1026062a47c6"), "SuccessfulIntroduceCode", new DateTime(2023, 11, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Number", false, 1, "Equal", null, "SideQuests", null, null, null },
                    { new Guid("d34cf82a-8582-4dd8-b76b-e89857f4c910"), "NinetyMinutesFocusMode", new DateTime(2023, 11, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Number", false, 1, "Equal", null, "SideQuests", null, null, null },
                    { new Guid("e0608ce3-6514-4fd2-88f6-f62db89e5fa7"), "OneHundredTwentytyMinutesFocusMode", new DateTime(2023, 11, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Number", false, 1, "Equal", null, "SideQuests", null, null, null },
                    { new Guid("e527e048-f02e-4a22-bec4-427b67c93d63"), "OneHundredEightyMinutesFocusMode", new DateTime(2023, 11, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Number", false, 1, "Equal", null, "SideQuests", null, null, null },
                    { new Guid("f62373aa-3fde-4848-8fef-7567fd0c7e8b"), "SixtyMinutesFocusMode", new DateTime(2023, 11, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Number", false, 1, "Equal", null, "SideQuests", null, null, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "QuestBoardConfigs",
                keyColumn: "Id",
                keyValue: new Guid("0417a848-1cd0-4aef-ae33-2757652701d0"));

            migrationBuilder.DeleteData(
                table: "QuestBoardConfigs",
                keyColumn: "Id",
                keyValue: new Guid("0aee60ff-65bc-4277-803c-8a21c38b2b84"));

            migrationBuilder.DeleteData(
                table: "QuestBoardConfigs",
                keyColumn: "Id",
                keyValue: new Guid("c3c2c8aa-fc4a-4b01-8d55-1026062a47c6"));

            migrationBuilder.DeleteData(
                table: "QuestBoardConfigs",
                keyColumn: "Id",
                keyValue: new Guid("d34cf82a-8582-4dd8-b76b-e89857f4c910"));

            migrationBuilder.DeleteData(
                table: "QuestBoardConfigs",
                keyColumn: "Id",
                keyValue: new Guid("e0608ce3-6514-4fd2-88f6-f62db89e5fa7"));

            migrationBuilder.DeleteData(
                table: "QuestBoardConfigs",
                keyColumn: "Id",
                keyValue: new Guid("e527e048-f02e-4a22-bec4-427b67c93d63"));

            migrationBuilder.DeleteData(
                table: "QuestBoardConfigs",
                keyColumn: "Id",
                keyValue: new Guid("f62373aa-3fde-4848-8fef-7567fd0c7e8b"));

            migrationBuilder.InsertData(
                table: "QuestBoardConfigs",
                columns: new[] { "Id", "Category", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "DisplayType", "IsDeleted", "MaxPoints", "Operator", "TaskPageUrl", "Type", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[,]
                {
                    { new Guid("0a20cb15-8629-41d9-88d9-890a3b487496"), "ThirtyMinutesFocusMode", new DateTime(2023, 11, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Number", false, 1, "Equal", null, "SideQuests", null, null, null },
                    { new Guid("0ddf9f6f-d2cb-49c2-858a-f4afb3233dee"), "SuccessfulIntroduceCode", new DateTime(2023, 11, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Number", false, 1, "Equal", null, "SideQuests", null, null, null },
                    { new Guid("2ca29e0c-4f81-40c6-9a0c-040d270d0433"), "NinetyMinutesFocusMode", new DateTime(2023, 11, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Number", false, 1, "Equal", null, "SideQuests", null, null, null },
                    { new Guid("406c775b-51e8-4742-8c4b-a898ae2b8924"), "OneHundredTwentytyMinutesFocusMode", new DateTime(2023, 11, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Number", false, 1, "Equal", null, "SideQuests", null, null, null },
                    { new Guid("62767c8d-6e18-4d11-a74e-fe38eab7fa58"), "SixtyMinutesFocusMode", new DateTime(2023, 11, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Number", false, 1, "Equal", null, "SideQuests", null, null, null },
                    { new Guid("e1c02bbc-0c98-40a0-b7e0-807b5f137c3b"), "OneHundredEightyMinutesFocusMode", new DateTime(2023, 11, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Number", false, 1, "Equal", null, "SideQuests", null, null, null }
                });
        }
    }
}
