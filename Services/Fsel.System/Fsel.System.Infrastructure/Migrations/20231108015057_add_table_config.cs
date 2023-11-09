using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Fsel.System.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class add_table_config : Migration
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
                    { new Guid("55c8d2c2-85e0-40da-a3e2-42f17e902ec2"), "DoneOneHundredTwentytyMinutesFocusMode", new DateTime(2023, 11, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Number", false, 1, "Equal", null, "SideQuests", null, null, null },
                    { new Guid("57e3e603-824e-4b40-aeed-ec8fc75174cd"), "DoneOneHundredEightyMinutesFocusMode", new DateTime(2023, 11, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Number", false, 1, "Equal", null, "SideQuests", null, null, null },
                    { new Guid("5dd2a169-c8cc-47f3-93f3-1d6d73e02655"), "DoneNinetyMinutesFocusMode", new DateTime(2023, 11, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Number", false, 1, "Equal", null, "SideQuests", null, null, null },
                    { new Guid("7cab5602-c49e-4619-9928-d1a2a9b63feb"), "DoneThirtyMinutesFocusMode", new DateTime(2023, 11, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Number", false, 1, "Equal", null, "SideQuests", null, null, null },
                    { new Guid("818e1492-f24b-4137-b199-d99b70b6a938"), "DoneSixtyMinutesFocusMode", new DateTime(2023, 11, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Number", false, 1, "Equal", null, "SideQuests", null, null, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
