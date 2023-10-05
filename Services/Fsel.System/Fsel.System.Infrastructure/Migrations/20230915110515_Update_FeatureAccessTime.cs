using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Fsel.System.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_FeatureAccessTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.CreateTable(
                name: "FeatureAccessTimes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedFullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UpdatedFullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DeletedFullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    EnumFeature = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Visit = table.Column<int>(type: "int", nullable: false),
                    AccessTime = table.Column<long>(type: "bigint", nullable: false),
                    LastVisited = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ObjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeatureAccessTimes", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FeatureAccessTimes");

            migrationBuilder.InsertData(
                table: "QuestBoardConfigs",
                columns: new[] { "Id", "Category", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "IsDeleted", "Type", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[,]
                {
                    { new Guid("64c0a3a5-849c-412d-86b6-3f5e4809cc84"), "FinishOneFinalTest", new DateTime(2023, 8, 18, 11, 35, 36, 608, DateTimeKind.Local).AddTicks(8248), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "MainQuests", null, null, null },
                    { new Guid("68bd35d3-c09b-45f3-9a05-0e10691f7c42"), "FinishOnelesson", new DateTime(2023, 8, 18, 11, 35, 36, 608, DateTimeKind.Local).AddTicks(7897), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "MainQuests", null, null, null },
                    { new Guid("72933280-e14b-4715-a802-dcd88e031e79"), "FinishOneHomeworkMiniProject", new DateTime(2023, 8, 18, 11, 35, 36, 608, DateTimeKind.Local).AddTicks(8193), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "MainQuests", null, null, null },
                    { new Guid("7b436441-ef2f-4a83-be79-ee5ec5f18355"), "FinishOneUnit", new DateTime(2023, 8, 18, 11, 35, 36, 608, DateTimeKind.Local).AddTicks(8237), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "MainQuests", null, null, null },
                    { new Guid("bb35cdbf-98a7-4ce7-9b71-f457b386d64c"), "FinishOneLevelPass", new DateTime(2023, 8, 18, 11, 35, 36, 608, DateTimeKind.Local).AddTicks(8260), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "MainQuests", null, null, null },
                    { new Guid("cfce3f4b-66d5-467d-8f57-50a089257bcb"), "FinishOneUnitTest", new DateTime(2023, 8, 18, 11, 35, 36, 608, DateTimeKind.Local).AddTicks(8222), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "MainQuests", null, null, null }
                });
        }
    }
}
