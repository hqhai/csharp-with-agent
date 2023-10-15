using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Fsel.System.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTableFocusTimeConfig_ChangeData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "FocusTimeConfigs",
                keyColumn: "Id",
                keyValue: new Guid("0a7b57f3-c964-4f1b-8986-df1579c5d08b"),
                columns: new[] { "CreatedDate", "TargetTime" },
                values: new object[] { new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), 1800.0 });

            migrationBuilder.UpdateData(
                table: "FocusTimeConfigs",
                keyColumn: "Id",
                keyValue: new Guid("124f4341-4c87-4e5f-ba4d-2481d8d36737"),
                columns: new[] { "CreatedDate", "TargetTime" },
                values: new object[] { new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), 5400.0 });

            migrationBuilder.UpdateData(
                table: "FocusTimeConfigs",
                keyColumn: "Id",
                keyValue: new Guid("82061293-c9d0-4598-99f1-8dfd8162b999"),
                columns: new[] { "CreatedDate", "TargetTime" },
                values: new object[] { new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), 7200.0 });

            migrationBuilder.UpdateData(
                table: "FocusTimeConfigs",
                keyColumn: "Id",
                keyValue: new Guid("9b4fa7b6-1af4-458b-82d9-621c1a88654a"),
                columns: new[] { "CreatedDate", "TargetTime" },
                values: new object[] { new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), 3600.0 });

            migrationBuilder.UpdateData(
                table: "FocusTimeConfigs",
                keyColumn: "Id",
                keyValue: new Guid("fd7e66d3-a29b-4da7-bb29-2283536d836a"),
                columns: new[] { "CreatedDate", "TargetTime" },
                values: new object[] { new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), 10800.0 });

            migrationBuilder.InsertData(
                table: "QuestBoardConfigs",
                columns: new[] { "Id", "Category", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "IsDeleted", "Type", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[,]
                {
                    { new Guid("64c0a3a5-849c-412d-86b6-3f5e4809cc84"), "FinishOneFinalTest", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "MainQuests", null, null, null },
                    { new Guid("68bd35d3-c09b-45f3-9a05-0e10691f7c42"), "FinishOneLesson", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "MainQuests", null, null, null },
                    { new Guid("72933280-e14b-4715-a802-dcd88e031e79"), "FinishOneHomeworkMiniProject", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "MainQuests", null, null, null },
                    { new Guid("7b436441-ef2f-4a83-be79-ee5ec5f18355"), "FinishOneUnit", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "MainQuests", null, null, null },
                    { new Guid("bb35cdbf-98a7-4ce7-9b71-f457b386d64c"), "FinishOneLevelPass", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "MainQuests", null, null, null },
                    { new Guid("cfce3f4b-66d5-467d-8f57-50a089257bcb"), "FinishOneUnitTest", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "MainQuests", null, null, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "QuestBoardConfigs",
                keyColumn: "Id",
                keyValue: new Guid("64c0a3a5-849c-412d-86b6-3f5e4809cc84"));

            migrationBuilder.DeleteData(
                table: "QuestBoardConfigs",
                keyColumn: "Id",
                keyValue: new Guid("68bd35d3-c09b-45f3-9a05-0e10691f7c42"));

            migrationBuilder.DeleteData(
                table: "QuestBoardConfigs",
                keyColumn: "Id",
                keyValue: new Guid("72933280-e14b-4715-a802-dcd88e031e79"));

            migrationBuilder.DeleteData(
                table: "QuestBoardConfigs",
                keyColumn: "Id",
                keyValue: new Guid("7b436441-ef2f-4a83-be79-ee5ec5f18355"));

            migrationBuilder.DeleteData(
                table: "QuestBoardConfigs",
                keyColumn: "Id",
                keyValue: new Guid("bb35cdbf-98a7-4ce7-9b71-f457b386d64c"));

            migrationBuilder.DeleteData(
                table: "QuestBoardConfigs",
                keyColumn: "Id",
                keyValue: new Guid("cfce3f4b-66d5-467d-8f57-50a089257bcb"));

            migrationBuilder.UpdateData(
                table: "FocusTimeConfigs",
                keyColumn: "Id",
                keyValue: new Guid("0a7b57f3-c964-4f1b-8986-df1579c5d08b"),
                columns: new[] { "CreatedDate", "TargetTime" },
                values: new object[] { new DateTime(2023, 10, 6, 18, 29, 32, 470, DateTimeKind.Local).AddTicks(669), 30.0 });

            migrationBuilder.UpdateData(
                table: "FocusTimeConfigs",
                keyColumn: "Id",
                keyValue: new Guid("124f4341-4c87-4e5f-ba4d-2481d8d36737"),
                columns: new[] { "CreatedDate", "TargetTime" },
                values: new object[] { new DateTime(2023, 10, 6, 18, 29, 32, 470, DateTimeKind.Local).AddTicks(1038), 90.0 });

            migrationBuilder.UpdateData(
                table: "FocusTimeConfigs",
                keyColumn: "Id",
                keyValue: new Guid("82061293-c9d0-4598-99f1-8dfd8162b999"),
                columns: new[] { "CreatedDate", "TargetTime" },
                values: new object[] { new DateTime(2023, 10, 6, 18, 29, 32, 470, DateTimeKind.Local).AddTicks(1054), 120.0 });

            migrationBuilder.UpdateData(
                table: "FocusTimeConfigs",
                keyColumn: "Id",
                keyValue: new Guid("9b4fa7b6-1af4-458b-82d9-621c1a88654a"),
                columns: new[] { "CreatedDate", "TargetTime" },
                values: new object[] { new DateTime(2023, 10, 6, 18, 29, 32, 470, DateTimeKind.Local).AddTicks(1014), 60.0 });

            migrationBuilder.UpdateData(
                table: "FocusTimeConfigs",
                keyColumn: "Id",
                keyValue: new Guid("fd7e66d3-a29b-4da7-bb29-2283536d836a"),
                columns: new[] { "CreatedDate", "TargetTime" },
                values: new object[] { new DateTime(2023, 10, 6, 18, 29, 32, 470, DateTimeKind.Local).AddTicks(1069), 180.0 });
        }
    }
}
