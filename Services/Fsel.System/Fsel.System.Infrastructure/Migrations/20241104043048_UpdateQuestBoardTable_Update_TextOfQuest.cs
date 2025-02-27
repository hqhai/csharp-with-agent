using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.System.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateQuestBoardTable_Update_TextOfQuest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "QuestBoards",
                keyColumn: "Id",
                keyValue: new Guid("9069f184-fd45-413e-b56a-79852879282e"),
                column: "Description",
                value: "Hoàn thành Mục tiêu học tập ngày đầu tiên");

            migrationBuilder.UpdateData(
                table: "QuestBoards",
                keyColumn: "Id",
                keyValue: new Guid("909cee82-d102-4eaa-8f51-519f4e4740f1"),
                column: "Description",
                value: "Hoàn thành 7 lần Mục tiêu học tập ngày");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "QuestBoards",
                keyColumn: "Id",
                keyValue: new Guid("9069f184-fd45-413e-b56a-79852879282e"),
                column: "Description",
                value: "Hoàn thành Focus Mode đầu tiên");

            migrationBuilder.UpdateData(
                table: "QuestBoards",
                keyColumn: "Id",
                keyValue: new Guid("909cee82-d102-4eaa-8f51-519f4e4740f1"),
                column: "Description",
                value: "Hoàn thành 7 lần Focus Mode");
        }
    }
}
