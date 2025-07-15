using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Identity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EditTableRole_UserRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Discription",
                table: "AspNetRoles",
                newName: "Description");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "AspNetUserRoles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "AspNetRoles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "AspNetRoles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LocationIdStr",
                table: "AspNetRoles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("069ae2a9-2729-4905-a8fa-c6c9f9172d1d"),
                columns: new[] { "DisplayOrder", "IsActive", "LocationIdStr" },
                values: new object[] { 0, false, null });

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("1eeba88e-ed0c-48c4-8d7e-a17c5a2cc3fe"),
                columns: new[] { "DisplayOrder", "IsActive", "LocationIdStr" },
                values: new object[] { 0, false, null });

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("24ce207d-8732-4a32-83ef-c5f05805f124"),
                columns: new[] { "DisplayOrder", "IsActive", "LocationIdStr" },
                values: new object[] { 0, false, null });

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("4cfaa242-a625-4e02-86d9-862d48a413c8"),
                columns: new[] { "DisplayOrder", "IsActive", "LocationIdStr" },
                values: new object[] { 0, false, null });

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("65ff6784-21d7-4986-8394-681cf711a4f7"),
                columns: new[] { "DisplayOrder", "IsActive", "LocationIdStr" },
                values: new object[] { 0, false, null });

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("69976022-5dbb-4292-bab6-e94b6701061e"),
                columns: new[] { "DisplayOrder", "IsActive", "LocationIdStr" },
                values: new object[] { 0, false, null });

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("73cd33e9-2f40-453a-b153-4ab59eaa6b73"),
                columns: new[] { "DisplayOrder", "IsActive", "LocationIdStr" },
                values: new object[] { 0, false, null });

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("7753a049-ba55-4901-bf6a-ae65ff9ac8fc"),
                columns: new[] { "DisplayOrder", "IsActive", "LocationIdStr" },
                values: new object[] { 0, false, null });

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("8d89a29a-b40a-4045-b0ed-679d7a5ff990"),
                columns: new[] { "DisplayOrder", "IsActive", "LocationIdStr" },
                values: new object[] { 0, false, null });

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("c8c28631-0bc0-4166-b062-4aff0d38e70c"),
                columns: new[] { "DisplayOrder", "IsActive", "LocationIdStr" },
                values: new object[] { 0, false, null });

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("caa21b08-7ddb-4951-9f2d-6a7fa256775c"),
                columns: new[] { "DisplayOrder", "IsActive", "LocationIdStr" },
                values: new object[] { 0, false, null });

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("e8fc0d37-1aae-4ae6-9dc0-1ffe864bef45"),
                columns: new[] { "DisplayOrder", "IsActive", "LocationIdStr" },
                values: new object[] { 0, false, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "AspNetUserRoles");

            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "AspNetRoles");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "AspNetRoles");

            migrationBuilder.DropColumn(
                name: "LocationIdStr",
                table: "AspNetRoles");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "AspNetRoles",
                newName: "Discription");
        }
    }
}
