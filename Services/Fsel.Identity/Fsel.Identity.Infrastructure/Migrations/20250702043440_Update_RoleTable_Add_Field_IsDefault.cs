using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Identity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_RoleTable_Add_Field_IsDefault : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDefault",
                table: "AspNetRoles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("069ae2a9-2729-4905-a8fa-c6c9f9172d1d"),
                column: "IsDefault",
                value: true);

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("1eeba88e-ed0c-48c4-8d7e-a17c5a2cc3fe"),
                column: "IsDefault",
                value: true);

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("24ce207d-8732-4a32-83ef-c5f05805f124"),
                column: "IsDefault",
                value: true);

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("4cfaa242-a625-4e02-86d9-862d48a413c8"),
                column: "IsDefault",
                value: true);

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("65ff6784-21d7-4986-8394-681cf711a4f7"),
                column: "IsDefault",
                value: true);

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("69976022-5dbb-4292-bab6-e94b6701061e"),
                column: "IsDefault",
                value: true);

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("73cd33e9-2f40-453a-b153-4ab59eaa6b73"),
                column: "IsDefault",
                value: true);

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("7753a049-ba55-4901-bf6a-ae65ff9ac8fc"),
                column: "IsDefault",
                value: false);

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("8d89a29a-b40a-4045-b0ed-679d7a5ff990"),
                column: "IsDefault",
                value: true);

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("c8c28631-0bc0-4166-b062-4aff0d38e70c"),
                column: "IsDefault",
                value: true);

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("caa21b08-7ddb-4951-9f2d-6a7fa256775c"),
                column: "IsDefault",
                value: true);

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("e8fc0d37-1aae-4ae6-9dc0-1ffe864bef45"),
                column: "IsDefault",
                value: true);

            migrationBuilder.Sql(@"
            UPDATE AspNetRoles
            SET IsDefault = 1
            WHERE Name = 'AdminSchool'
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDefault",
                table: "AspNetRoles");
        }
    }
}
