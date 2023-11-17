using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.System.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class update_questboard_config : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "QuestBoardConfigs",
                columns: new[] { "Id", "Category", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "DisplayType", "IsDeleted", "MaxPoints", "Operator", "TaskPageUrl", "Type", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[] { new Guid("012a3990-47bc-436f-8d7d-10fd824bd579"), "SeeAllReviewsAndFeedback", new DateTime(2023, 11, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Number", false, 1, "Equal", null, "PremiumQuests", null, null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "QuestBoardConfigs",
                keyColumn: "Id",
                keyValue: new Guid("012a3990-47bc-436f-8d7d-10fd824bd579"));
        }
    }
}
