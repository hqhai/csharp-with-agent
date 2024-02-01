using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Fsel.System.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDate_QuestBoardConfigs_Config_1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
