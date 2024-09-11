using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Ordering.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_PackageEventTable_Add_Field_Status : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "PackageEvents",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "PackageEvents",
                keyColumn: "Id",
                keyValue: new Guid("2496f21a-7e62-41a3-a294-89674a411e03"),
                column: "Status",
                value: "Active");

            migrationBuilder.UpdateData(
                table: "PackageEvents",
                keyColumn: "Id",
                keyValue: new Guid("7ec4ded7-afd4-4c84-8b71-12c8d89afabd"),
                column: "Status",
                value: "Active");

            migrationBuilder.UpdateData(
                table: "PackageEvents",
                keyColumn: "Id",
                keyValue: new Guid("ebe09800-afd8-4621-a490-7f1fecf0e54c"),
                column: "Status",
                value: "Active");

            migrationBuilder.UpdateData(
                table: "Packages",
                keyColumn: "Id",
                keyValue: new Guid("42d7ddb2-9f36-4f86-badc-67dc16bb722b"),
                column: "Status",
                value: "Active");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "PackageEvents");

            migrationBuilder.UpdateData(
                table: "Packages",
                keyColumn: "Id",
                keyValue: new Guid("42d7ddb2-9f36-4f86-badc-67dc16bb722b"),
                column: "Status",
                value: "InActive");
        }
    }
}
