using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Fsel.Identity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRoleTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "3d4b05b9-b5dd-48f2-a125-f8a22228272e");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "7992932b-a03d-4eb6-8bd7-edadde25298e");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "d8842f4d-295f-4465-8531-4df80b79e1f2");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "AspNetRoles",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "AspNetRoles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Discriminator", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "4208aad6-1775-4454-9bff-857c51d14a1e", null, "IdentityRole", "CSO", "CSO" },
                    { "c3572c9b-bda3-4b93-8f52-107a6f7339e8", null, "IdentityRole", "MasterAdmin", "MasterAdmin" },
                    { "f925dab5-6404-46a4-b0cb-b3a3c816eb94", null, "IdentityRole", "Admin", "Admin" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "4208aad6-1775-4454-9bff-857c51d14a1e");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "c3572c9b-bda3-4b93-8f52-107a6f7339e8");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "f925dab5-6404-46a4-b0cb-b3a3c816eb94");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "AspNetRoles");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "AspNetRoles");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "3d4b05b9-b5dd-48f2-a125-f8a22228272e", null, "CSO", "CSO" },
                    { "7992932b-a03d-4eb6-8bd7-edadde25298e", null, "Admin", "Admin" },
                    { "d8842f4d-295f-4465-8531-4df80b79e1f2", null, "MasterAdmin", "MasterAdmin" }
                });
        }
    }
}
