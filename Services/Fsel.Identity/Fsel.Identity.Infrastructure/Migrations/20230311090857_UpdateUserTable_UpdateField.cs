using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Fsel.Identity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserTable_UpdateField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a010f9ec-352a-4c9f-a3bd-4f64d6ea9efd");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a11365f8-b8d1-4db6-a2d5-655d3b244a7a");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "c695f3d4-5393-4465-b564-6dca656fa258");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "CreatedUserId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "CreatedUserName",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "DeletedUserId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "DeletedUserName",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "UpdatedUserId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "UpdatedUserName",
                table: "AspNetUsers");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified))
                .Annotation("Relational:ColumnOrder", 107);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedUserId",
                table: "AspNetUsers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"))
                .Annotation("Relational:ColumnOrder", 101);

            migrationBuilder.AddColumn<string>(
                name: "CreatedUserName",
                table: "AspNetUsers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "")
                .Annotation("Relational:ColumnOrder", 104);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDate",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 109);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedUserId",
                table: "AspNetUsers",
                type: "uniqueidentifier",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 103);

            migrationBuilder.AddColumn<string>(
                name: "DeletedUserName",
                table: "AspNetUsers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("Relational:ColumnOrder", 106);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false)
                .Annotation("Relational:ColumnOrder", 110);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 108);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedUserId",
                table: "AspNetUsers",
                type: "uniqueidentifier",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 102);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedUserName",
                table: "AspNetUsers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("Relational:ColumnOrder", 105);

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "a010f9ec-352a-4c9f-a3bd-4f64d6ea9efd", null, "MasterAdmin", "MasterAdmin" },
                    { "a11365f8-b8d1-4db6-a2d5-655d3b244a7a", null, "CSO", "CSO" },
                    { "c695f3d4-5393-4465-b564-6dca656fa258", null, "Admin", "Admin" }
                });
        }
    }
}
