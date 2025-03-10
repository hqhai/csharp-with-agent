using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.System.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateQuestBoardTable_Update_CompleteTheFirstClassForum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "QuestBoards",
                keyColumn: "Id",
                keyValue: new Guid("1483c74d-d798-46e7-b152-51afd03ff88e"),
                column: "Description",
                value: "Hoàn thành đăng bài trong Diễn đàn lớp học đầu tiên");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "QuestBoards",
                keyColumn: "Id",
                keyValue: new Guid("1483c74d-d798-46e7-b152-51afd03ff88e"),
                column: "Description",
                value: "Hoàn thành Diễn đàn lớp học đầu tiên");
        }
    }
}
