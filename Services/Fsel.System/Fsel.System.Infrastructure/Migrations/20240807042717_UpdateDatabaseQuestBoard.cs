using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.System.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDatabaseQuestBoard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "QuestBoardOveralls",
                keyColumn: "Id",
                keyValue: new Guid("59c4f992-cc12-4cfc-baf8-4929de320bc2"),
                column: "TargetValue",
                value: 7);

            migrationBuilder.UpdateData(
                table: "QuestBoards",
                keyColumn: "Id",
                keyValue: new Guid("6942432d-f45c-45de-a72c-002c0a04cc8c"),
                column: "Token",
                value: 20);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "QuestBoardOveralls",
                keyColumn: "Id",
                keyValue: new Guid("59c4f992-cc12-4cfc-baf8-4929de320bc2"),
                column: "TargetValue",
                value: 6);

            migrationBuilder.UpdateData(
                table: "QuestBoards",
                keyColumn: "Id",
                keyValue: new Guid("6942432d-f45c-45de-a72c-002c0a04cc8c"),
                column: "Token",
                value: 40);
        }
    }
}
