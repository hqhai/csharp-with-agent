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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
