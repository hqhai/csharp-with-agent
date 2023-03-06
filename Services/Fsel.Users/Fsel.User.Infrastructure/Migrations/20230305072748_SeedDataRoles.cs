using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Fsel.User.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedDataRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
        }
    }
}
