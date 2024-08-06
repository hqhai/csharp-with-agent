using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.System.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateNewDataQuestBoard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "QuestBoards",
                keyColumn: "Id",
                keyValue: new Guid("9069f184-fd45-413e-b56a-79852879282e"),
                column: "Name",
                value: "Tân thủ VI");

            migrationBuilder.UpdateData(
                table: "QuestBoards",
                keyColumn: "Id",
                keyValue: new Guid("c77a4195-ee46-4639-990e-6aa6e25284fa"),
                column: "Name",
                value: "Tân thủ VII");

            migrationBuilder.InsertData(
                table: "QuestBoards",
                columns: new[] { "Id", "Category", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "Description", "Energy", "ImagePath", "IsActive", "IsDeleted", "Name", "RepeatType", "TargetValue", "Token", "Type", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[] { new Guid("6942432d-f45c-45de-a72c-002c0a04cc8c"), "CompletedSurvey", new DateTime(2024, 5, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Hoàn thành Khảo sát thông tin", null, "", true, false, "Tân thủ V", null, 1, 40, "BeginnerQuests", null, null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "QuestBoards",
                keyColumn: "Id",
                keyValue: new Guid("6942432d-f45c-45de-a72c-002c0a04cc8c"));

            migrationBuilder.UpdateData(
                table: "QuestBoards",
                keyColumn: "Id",
                keyValue: new Guid("9069f184-fd45-413e-b56a-79852879282e"),
                column: "Name",
                value: "Tân thủ V");

            migrationBuilder.UpdateData(
                table: "QuestBoards",
                keyColumn: "Id",
                keyValue: new Guid("c77a4195-ee46-4639-990e-6aa6e25284fa"),
                column: "Name",
                value: "Tân thủ VI");
        }
    }
}
