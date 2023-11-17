using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.System.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateQuestBoardConfigTable_V1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "QuestBoardConfigs",
                keyColumn: "Id",
                keyValue: new Guid("0407d174-a779-46e9-bbe4-5f27ae6075b6"),
                columns: new[] { "MaxPoints", "Operator" },
                values: new object[] { 80, "GreaterThan" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "QuestBoardConfigs",
                keyColumn: "Id",
                keyValue: new Guid("0407d174-a779-46e9-bbe4-5f27ae6075b6"),
                columns: new[] { "MaxPoints", "Operator" },
                values: new object[] { 100, "Equal" });
        }
    }
}
