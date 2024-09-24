using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.System.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_QuesyBoardTable_Update_Field_RepeatType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "QuestBoards",
                keyColumn: "Id",
                keyValue: new Guid("0ac5668a-1d21-4c05-96b6-94012d183ae6"),
                column: "RepeatType",
                value: "Weekly");

            migrationBuilder.UpdateData(
                table: "QuestBoards",
                keyColumn: "Id",
                keyValue: new Guid("1a467647-049c-47c7-a596-8ff5bf33ff10"),
                column: "RepeatType",
                value: "Daily");

            migrationBuilder.UpdateData(
                table: "QuestBoards",
                keyColumn: "Id",
                keyValue: new Guid("393dcd7e-9b4b-4a69-a222-6981c3cc1967"),
                column: "RepeatType",
                value: "Daily");

            migrationBuilder.UpdateData(
                table: "QuestBoards",
                keyColumn: "Id",
                keyValue: new Guid("418e49eb-54ff-4e8a-b87a-368b9428d74b"),
                column: "RepeatType",
                value: "Daily");

            migrationBuilder.UpdateData(
                table: "QuestBoards",
                keyColumn: "Id",
                keyValue: new Guid("540839db-2511-4074-990f-d69a3b75fb90"),
                column: "RepeatType",
                value: "Daily");

            migrationBuilder.UpdateData(
                table: "QuestBoards",
                keyColumn: "Id",
                keyValue: new Guid("73421144-de83-4b9f-9eea-d0b2c8dc6530"),
                column: "RepeatType",
                value: "Daily");

            migrationBuilder.UpdateData(
                table: "QuestBoards",
                keyColumn: "Id",
                keyValue: new Guid("7372c5a5-4e53-4da4-a6f2-7c992abb014a"),
                column: "RepeatType",
                value: "Daily");

            migrationBuilder.UpdateData(
                table: "QuestBoards",
                keyColumn: "Id",
                keyValue: new Guid("909cee82-d102-4eaa-8f51-519f4e4740f1"),
                column: "RepeatType",
                value: "Weekly");

            migrationBuilder.UpdateData(
                table: "QuestBoards",
                keyColumn: "Id",
                keyValue: new Guid("91ef56a9-3638-4994-b136-019c9ec51120"),
                column: "RepeatType",
                value: "Daily");

            migrationBuilder.UpdateData(
                table: "QuestBoards",
                keyColumn: "Id",
                keyValue: new Guid("932ed712-dc2b-42dd-a700-de944018ad0e"),
                column: "RepeatType",
                value: "Daily");

            migrationBuilder.UpdateData(
                table: "QuestBoards",
                keyColumn: "Id",
                keyValue: new Guid("9f57609b-1f81-4a42-a104-5aa2dca21b2e"),
                column: "RepeatType",
                value: "Daily");

            migrationBuilder.UpdateData(
                table: "QuestBoards",
                keyColumn: "Id",
                keyValue: new Guid("b6be6e8b-3317-4f25-96b7-8ad1fa151793"),
                column: "RepeatType",
                value: "Weekly");

            migrationBuilder.UpdateData(
                table: "QuestBoards",
                keyColumn: "Id",
                keyValue: new Guid("d33f9400-adf9-431c-b97c-e679321f4f17"),
                column: "RepeatType",
                value: "Weekly");

            migrationBuilder.UpdateData(
                table: "QuestBoards",
                keyColumn: "Id",
                keyValue: new Guid("e6fc1b44-57aa-44bc-989f-5e34d4152b9d"),
                column: "RepeatType",
                value: "Weekly");

            migrationBuilder.UpdateData(
                table: "QuestBoards",
                keyColumn: "Id",
                keyValue: new Guid("eec844f3-2574-4a0d-a6a6-6b12f07264dd"),
                column: "RepeatType",
                value: "Daily");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "QuestBoards",
                keyColumn: "Id",
                keyValue: new Guid("0ac5668a-1d21-4c05-96b6-94012d183ae6"),
                column: "RepeatType",
                value: "Week");

            migrationBuilder.UpdateData(
                table: "QuestBoards",
                keyColumn: "Id",
                keyValue: new Guid("1a467647-049c-47c7-a596-8ff5bf33ff10"),
                column: "RepeatType",
                value: "Day");

            migrationBuilder.UpdateData(
                table: "QuestBoards",
                keyColumn: "Id",
                keyValue: new Guid("393dcd7e-9b4b-4a69-a222-6981c3cc1967"),
                column: "RepeatType",
                value: "Day");

            migrationBuilder.UpdateData(
                table: "QuestBoards",
                keyColumn: "Id",
                keyValue: new Guid("418e49eb-54ff-4e8a-b87a-368b9428d74b"),
                column: "RepeatType",
                value: "Day");

            migrationBuilder.UpdateData(
                table: "QuestBoards",
                keyColumn: "Id",
                keyValue: new Guid("540839db-2511-4074-990f-d69a3b75fb90"),
                column: "RepeatType",
                value: "Day");

            migrationBuilder.UpdateData(
                table: "QuestBoards",
                keyColumn: "Id",
                keyValue: new Guid("73421144-de83-4b9f-9eea-d0b2c8dc6530"),
                column: "RepeatType",
                value: "Day");

            migrationBuilder.UpdateData(
                table: "QuestBoards",
                keyColumn: "Id",
                keyValue: new Guid("7372c5a5-4e53-4da4-a6f2-7c992abb014a"),
                column: "RepeatType",
                value: "Day");

            migrationBuilder.UpdateData(
                table: "QuestBoards",
                keyColumn: "Id",
                keyValue: new Guid("909cee82-d102-4eaa-8f51-519f4e4740f1"),
                column: "RepeatType",
                value: "Week");

            migrationBuilder.UpdateData(
                table: "QuestBoards",
                keyColumn: "Id",
                keyValue: new Guid("91ef56a9-3638-4994-b136-019c9ec51120"),
                column: "RepeatType",
                value: "Day");

            migrationBuilder.UpdateData(
                table: "QuestBoards",
                keyColumn: "Id",
                keyValue: new Guid("932ed712-dc2b-42dd-a700-de944018ad0e"),
                column: "RepeatType",
                value: "Day");

            migrationBuilder.UpdateData(
                table: "QuestBoards",
                keyColumn: "Id",
                keyValue: new Guid("9f57609b-1f81-4a42-a104-5aa2dca21b2e"),
                column: "RepeatType",
                value: "Day");

            migrationBuilder.UpdateData(
                table: "QuestBoards",
                keyColumn: "Id",
                keyValue: new Guid("b6be6e8b-3317-4f25-96b7-8ad1fa151793"),
                column: "RepeatType",
                value: "Week");

            migrationBuilder.UpdateData(
                table: "QuestBoards",
                keyColumn: "Id",
                keyValue: new Guid("d33f9400-adf9-431c-b97c-e679321f4f17"),
                column: "RepeatType",
                value: "Week");

            migrationBuilder.UpdateData(
                table: "QuestBoards",
                keyColumn: "Id",
                keyValue: new Guid("e6fc1b44-57aa-44bc-989f-5e34d4152b9d"),
                column: "RepeatType",
                value: "Week");

            migrationBuilder.UpdateData(
                table: "QuestBoards",
                keyColumn: "Id",
                keyValue: new Guid("eec844f3-2574-4a0d-a6a6-6b12f07264dd"),
                column: "RepeatType",
                value: "Day");
        }
    }
}
