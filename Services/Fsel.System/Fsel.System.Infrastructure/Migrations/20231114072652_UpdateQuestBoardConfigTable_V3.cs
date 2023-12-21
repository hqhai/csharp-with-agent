using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Fsel.System.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateQuestBoardConfigTable_V3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "QuestBoardConfigs",
                columns: new[] { "Id", "Category", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "DisplayType", "IsDeleted", "MaxPoints", "Operator", "TaskPageUrl", "Type", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[,]
                {
                    { new Guid("2ea59046-ea24-4f3f-b390-36d9629ae11e"), "CommentOnNewLessonOfTwoClassMate", new DateTime(2023, 11, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Number", false, 1, "Equal", null, "DailyQuests", null, null, null },
                    { new Guid("31c2d3aa-ec1d-4ebb-99d1-5a8a2ae391c7"), "LearnInteractTwentyMinutes", new DateTime(2023, 11, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Number", false, 1, "Equal", null, "DailyQuests", null, null, null },
                    { new Guid("38045664-20f8-4780-9c62-9736c2bce90c"), "FinishDailyFocusMode", new DateTime(2023, 11, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Number", false, 1, "Equal", null, "DailyQuests", null, null, null },
                    { new Guid("9d333771-8c0d-481c-95e7-542a12002684"), "CompleteHomeWorkAtLeastFiftyPercent", new DateTime(2023, 11, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Number", false, 1, "Equal", null, "DailyQuests", null, null, null },
                    { new Guid("f8c5416c-118e-45dc-942c-f61e985b9827"), "ReviseYourNotes", new DateTime(2023, 11, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Number", false, 1, "Equal", null, "DailyQuests", null, null, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "QuestBoardConfigs",
                keyColumn: "Id",
                keyValue: new Guid("2ea59046-ea24-4f3f-b390-36d9629ae11e"));

            migrationBuilder.DeleteData(
                table: "QuestBoardConfigs",
                keyColumn: "Id",
                keyValue: new Guid("31c2d3aa-ec1d-4ebb-99d1-5a8a2ae391c7"));

            migrationBuilder.DeleteData(
                table: "QuestBoardConfigs",
                keyColumn: "Id",
                keyValue: new Guid("38045664-20f8-4780-9c62-9736c2bce90c"));

            migrationBuilder.DeleteData(
                table: "QuestBoardConfigs",
                keyColumn: "Id",
                keyValue: new Guid("9d333771-8c0d-481c-95e7-542a12002684"));

            migrationBuilder.DeleteData(
                table: "QuestBoardConfigs",
                keyColumn: "Id",
                keyValue: new Guid("f8c5416c-118e-45dc-942c-f61e985b9827"));
        }
    }
}
