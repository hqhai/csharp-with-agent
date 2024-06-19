using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Fsel.System.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addquest_board_config : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "QuestBoardConfigs",
                keyColumn: "Id",
                keyValue: new Guid("55c8d2c2-85e0-40da-a3e2-42f17e902ec2"));

            migrationBuilder.DeleteData(
                table: "QuestBoardConfigs",
                keyColumn: "Id",
                keyValue: new Guid("57e3e603-824e-4b40-aeed-ec8fc75174cd"));

            migrationBuilder.DeleteData(
                table: "QuestBoardConfigs",
                keyColumn: "Id",
                keyValue: new Guid("5dd2a169-c8cc-47f3-93f3-1d6d73e02655"));

            migrationBuilder.DeleteData(
                table: "QuestBoardConfigs",
                keyColumn: "Id",
                keyValue: new Guid("7cab5602-c49e-4619-9928-d1a2a9b63feb"));

            migrationBuilder.DeleteData(
                table: "QuestBoardConfigs",
                keyColumn: "Id",
                keyValue: new Guid("818e1492-f24b-4137-b199-d99b70b6a938"));

            migrationBuilder.InsertData(
                table: "QuestBoardConfigs",
                columns: new[] { "Id", "Category", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "DisplayType", "IsDeleted", "MaxPoints", "Operator", "TaskPageUrl", "Type", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[,]
                {
                    { new Guid("0a20cb15-8629-41d9-88d9-890a3b487496"), "ThirtyMinutesFocusMode", new DateTime(2023, 11, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Number", false, 1, "Equal", null, "SideQuests", null, null, null },
                    { new Guid("2ca29e0c-4f81-40c6-9a0c-040d270d0433"), "NinetyMinutesFocusMode", new DateTime(2023, 11, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Number", false, 1, "Equal", null, "SideQuests", null, null, null },
                    { new Guid("406c775b-51e8-4742-8c4b-a898ae2b8924"), "OneHundredTwentytyMinutesFocusMode", new DateTime(2023, 11, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Number", false, 1, "Equal", null, "SideQuests", null, null, null },
                    { new Guid("62767c8d-6e18-4d11-a74e-fe38eab7fa58"), "SixtyMinutesFocusMode", new DateTime(2023, 11, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Number", false, 1, "Equal", null, "SideQuests", null, null, null },
                    { new Guid("e1c02bbc-0c98-40a0-b7e0-807b5f137c3b"), "OneHundredEightyMinutesFocusMode", new DateTime(2023, 11, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Number", false, 1, "Equal", null, "SideQuests", null, null, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "QuestBoardConfigs",
                keyColumn: "Id",
                keyValue: new Guid("0a20cb15-8629-41d9-88d9-890a3b487496"));

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
                    { new Guid("55c8d2c2-85e0-40da-a3e2-42f17e902ec2"), "DoneOneHundredTwentytyMinutesFocusMode", new DateTime(2023, 11, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Number", false, 1, "Equal", null, "SideQuests", null, null, null },
                    { new Guid("57e3e603-824e-4b40-aeed-ec8fc75174cd"), "DoneOneHundredEightyMinutesFocusMode", new DateTime(2023, 11, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Number", false, 1, "Equal", null, "SideQuests", null, null, null },
                    { new Guid("5dd2a169-c8cc-47f3-93f3-1d6d73e02655"), "DoneNinetyMinutesFocusMode", new DateTime(2023, 11, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Number", false, 1, "Equal", null, "SideQuests", null, null, null },
                    { new Guid("7cab5602-c49e-4619-9928-d1a2a9b63feb"), "DoneThirtyMinutesFocusMode", new DateTime(2023, 11, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Number", false, 1, "Equal", null, "SideQuests", null, null, null },
                    { new Guid("818e1492-f24b-4137-b199-d99b70b6a938"), "DoneSixtyMinutesFocusMode", new DateTime(2023, 11, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Number", false, 1, "Equal", null, "SideQuests", null, null, null }
                });
        }
    }
}
