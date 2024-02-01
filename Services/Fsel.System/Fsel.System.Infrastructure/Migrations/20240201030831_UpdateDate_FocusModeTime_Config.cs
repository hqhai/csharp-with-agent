using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Fsel.System.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDate_FocusModeTime_Config : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "FocusTimeConfigs",
                keyColumn: "Id",
                keyValue: new Guid("0a7b57f3-c964-4f1b-8986-df1579c5d08b"),
                column: "TargetTime",
                value: 900.0);

            migrationBuilder.UpdateData(
                table: "FocusTimeConfigs",
                keyColumn: "Id",
                keyValue: new Guid("124f4341-4c87-4e5f-ba4d-2481d8d36737"),
                column: "TargetTime",
                value: 2700.0);

            migrationBuilder.UpdateData(
                table: "FocusTimeConfigs",
                keyColumn: "Id",
                keyValue: new Guid("82061293-c9d0-4598-99f1-8dfd8162b999"),
                column: "TargetTime",
                value: 3600.0);

            migrationBuilder.UpdateData(
                table: "FocusTimeConfigs",
                keyColumn: "Id",
                keyValue: new Guid("9b4fa7b6-1af4-458b-82d9-621c1a88654a"),
                column: "TargetTime",
                value: 1800.0);

            migrationBuilder.UpdateData(
                table: "FocusTimeConfigs",
                keyColumn: "Id",
                keyValue: new Guid("fd7e66d3-a29b-4da7-bb29-2283536d836a"),
                column: "TargetTime",
                value: 5400.0);

            migrationBuilder.UpdateData(
                table: "QuestBoardConfigs",
                keyColumn: "Id",
                keyValue: new Guid("2ea59046-ea24-4f3f-b390-36d9629ae11e"),
                column: "MaxPoints",
                value: 2);

            migrationBuilder.UpdateData(
                table: "QuestBoardConfigs",
                keyColumn: "Id",
                keyValue: new Guid("cfce3f4b-66d5-467d-8f57-50a089257bcb"),
                column: "Category",
                value: "FinishOneLesson");

            migrationBuilder.InsertData(
                table: "QuestBoardConfigs",
                columns: new[] { "Id", "Category", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "DisplayType", "IsDeleted", "MaxPoints", "Operator", "TaskPageUrl", "Type", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[,]
                {
                    { new Guid("012a3990-47bc-436f-8d7d-10fd824bd579"), "SeeAllReviewsAndFeedback", new DateTime(2023, 11, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Number", false, 1, "Equal", null, "PremiumQuests", null, null, null },
                    { new Guid("0417a848-1cd0-4aef-ae33-2757652701d0"), "RateAndComment", new DateTime(2023, 11, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Number", false, 1, "Equal", null, "SideQuests", null, null, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "QuestBoardConfigs",
                keyColumn: "Id",
                keyValue: new Guid("012a3990-47bc-436f-8d7d-10fd824bd579"));

            migrationBuilder.DeleteData(
                table: "QuestBoardConfigs",
                keyColumn: "Id",
                keyValue: new Guid("0417a848-1cd0-4aef-ae33-2757652701d0"));

            migrationBuilder.UpdateData(
                table: "FocusTimeConfigs",
                keyColumn: "Id",
                keyValue: new Guid("0a7b57f3-c964-4f1b-8986-df1579c5d08b"),
                column: "TargetTime",
                value: 1800.0);

            migrationBuilder.UpdateData(
                table: "FocusTimeConfigs",
                keyColumn: "Id",
                keyValue: new Guid("124f4341-4c87-4e5f-ba4d-2481d8d36737"),
                column: "TargetTime",
                value: 5400.0);

            migrationBuilder.UpdateData(
                table: "FocusTimeConfigs",
                keyColumn: "Id",
                keyValue: new Guid("82061293-c9d0-4598-99f1-8dfd8162b999"),
                column: "TargetTime",
                value: 7200.0);

            migrationBuilder.UpdateData(
                table: "FocusTimeConfigs",
                keyColumn: "Id",
                keyValue: new Guid("9b4fa7b6-1af4-458b-82d9-621c1a88654a"),
                column: "TargetTime",
                value: 3600.0);

            migrationBuilder.UpdateData(
                table: "FocusTimeConfigs",
                keyColumn: "Id",
                keyValue: new Guid("fd7e66d3-a29b-4da7-bb29-2283536d836a"),
                column: "TargetTime",
                value: 10800.0);

            migrationBuilder.UpdateData(
                table: "QuestBoardConfigs",
                keyColumn: "Id",
                keyValue: new Guid("2ea59046-ea24-4f3f-b390-36d9629ae11e"),
                column: "MaxPoints",
                value: 1);

            migrationBuilder.UpdateData(
                table: "QuestBoardConfigs",
                keyColumn: "Id",
                keyValue: new Guid("cfce3f4b-66d5-467d-8f57-50a089257bcb"),
                column: "Category",
                value: "FinishOneUnitTest");
        }
    }
}
