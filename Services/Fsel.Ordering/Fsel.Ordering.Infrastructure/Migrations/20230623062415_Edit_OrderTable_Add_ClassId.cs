using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Ordering.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Edit_OrderTable_Add_ClassId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ClassId",
                table: "Orders",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.UpdateData(
                table: "Packages",
                keyColumn: "Id",
                keyValue: new Guid("42d7ddb2-9f36-4f86-badc-67dc16bb722b"),
                column: "CreatedDate",
                value: new DateTime(2023, 6, 23, 13, 24, 14, 971, DateTimeKind.Local).AddTicks(7825));

            migrationBuilder.UpdateData(
                table: "Packages",
                keyColumn: "Id",
                keyValue: new Guid("d13ee4ab-785a-425c-bd70-b74b61df42eb"),
                column: "CreatedDate",
                value: new DateTime(2023, 6, 23, 13, 24, 14, 973, DateTimeKind.Local).AddTicks(5322));

            migrationBuilder.UpdateData(
                table: "Packages",
                keyColumn: "Id",
                keyValue: new Guid("daa6fc87-6461-49d4-b3a5-c9e4cc30bc59"),
                column: "CreatedDate",
                value: new DateTime(2023, 6, 23, 13, 24, 14, 973, DateTimeKind.Local).AddTicks(3622));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClassId",
                table: "Orders");

            migrationBuilder.UpdateData(
                table: "Packages",
                keyColumn: "Id",
                keyValue: new Guid("42d7ddb2-9f36-4f86-badc-67dc16bb722b"),
                column: "CreatedDate",
                value: new DateTime(2023, 6, 7, 9, 6, 15, 722, DateTimeKind.Local).AddTicks(4916));

            migrationBuilder.UpdateData(
                table: "Packages",
                keyColumn: "Id",
                keyValue: new Guid("d13ee4ab-785a-425c-bd70-b74b61df42eb"),
                column: "CreatedDate",
                value: new DateTime(2023, 6, 7, 9, 6, 15, 741, DateTimeKind.Local).AddTicks(8989));

            migrationBuilder.UpdateData(
                table: "Packages",
                keyColumn: "Id",
                keyValue: new Guid("daa6fc87-6461-49d4-b3a5-c9e4cc30bc59"),
                column: "CreatedDate",
                value: new DateTime(2023, 6, 7, 9, 6, 15, 741, DateTimeKind.Local).AddTicks(2833));
        }
    }
}
