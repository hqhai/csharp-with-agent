using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Ordering.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTableVoucher : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomerType",
                table: "Vouchers");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Vouchers",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerTypesStr",
                table: "Vouchers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Packages",
                keyColumn: "Id",
                keyValue: new Guid("42d7ddb2-9f36-4f86-badc-67dc16bb722b"),
                column: "CreatedDate",
                value: new DateTime(2023, 7, 24, 15, 42, 49, 752, DateTimeKind.Local).AddTicks(4373));

            migrationBuilder.UpdateData(
                table: "Packages",
                keyColumn: "Id",
                keyValue: new Guid("d13ee4ab-785a-425c-bd70-b74b61df42eb"),
                column: "CreatedDate",
                value: new DateTime(2023, 7, 24, 15, 42, 49, 752, DateTimeKind.Local).AddTicks(8790));

            migrationBuilder.UpdateData(
                table: "Packages",
                keyColumn: "Id",
                keyValue: new Guid("daa6fc87-6461-49d4-b3a5-c9e4cc30bc59"),
                column: "CreatedDate",
                value: new DateTime(2023, 7, 24, 15, 42, 49, 752, DateTimeKind.Local).AddTicks(7637));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomerTypesStr",
                table: "Vouchers");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Vouchers",
                type: "bit",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AddColumn<string>(
                name: "CustomerType",
                table: "Vouchers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Packages",
                keyColumn: "Id",
                keyValue: new Guid("42d7ddb2-9f36-4f86-badc-67dc16bb722b"),
                column: "CreatedDate",
                value: new DateTime(2023, 7, 5, 13, 22, 48, 175, DateTimeKind.Local).AddTicks(860));

            migrationBuilder.UpdateData(
                table: "Packages",
                keyColumn: "Id",
                keyValue: new Guid("d13ee4ab-785a-425c-bd70-b74b61df42eb"),
                column: "CreatedDate",
                value: new DateTime(2023, 7, 5, 13, 22, 48, 176, DateTimeKind.Local).AddTicks(5651));

            migrationBuilder.UpdateData(
                table: "Packages",
                keyColumn: "Id",
                keyValue: new Guid("daa6fc87-6461-49d4-b3a5-c9e4cc30bc59"),
                column: "CreatedDate",
                value: new DateTime(2023, 7, 5, 13, 22, 48, 176, DateTimeKind.Local).AddTicks(4248));
        }
    }
}
