using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Fsel.User.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_AccountTable_RequiedFullNameField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "3e515455-9419-45ac-a807-34901413d06b");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "5114eddd-7c9a-4e82-973c-2935f6b4f4c0");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a8167e70-3d29-448b-b84f-f6b4faf741ec");

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "1341158d-acf4-4f0a-ac9a-effe17d56cbb", null, "Admin", "Admin" },
                    { "177d29f4-a424-41e3-ad35-fba822a6480f", null, "MasterAdmin", "MasterAdmin" },
                    { "a586e3a2-1112-41bc-b282-bea8b8c7c0bb", null, "CSO", "CSO" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "1341158d-acf4-4f0a-ac9a-effe17d56cbb");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "177d29f4-a424-41e3-ad35-fba822a6480f");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a586e3a2-1112-41bc-b282-bea8b8c7c0bb");

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "3e515455-9419-45ac-a807-34901413d06b", null, "CSO", "CSO" },
                    { "5114eddd-7c9a-4e82-973c-2935f6b4f4c0", null, "Admin", "Admin" },
                    { "a8167e70-3d29-448b-b84f-f6b4faf741ec", null, "MasterAdmin", "MasterAdmin" }
                });
        }
    }
}
