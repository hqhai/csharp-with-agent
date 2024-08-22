using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Ordering.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_PackageTable_Delete_Fields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MonthBonusNumber",
                table: "Packages");

            migrationBuilder.DropColumn(
                name: "Suggest",
                table: "Packages");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "MonthBonusNumber",
                table: "Packages",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "Suggest",
                table: "Packages",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Packages",
                keyColumn: "Id",
                keyValue: new Guid("42d7ddb2-9f36-4f86-badc-67dc16bb722b"),
                columns: new[] { "MonthBonusNumber", "Suggest" },
                values: new object[] { 0.0, null });

            migrationBuilder.UpdateData(
                table: "Packages",
                keyColumn: "Id",
                keyValue: new Guid("d13ee4ab-785a-425c-bd70-b74b61df42eb"),
                columns: new[] { "MonthBonusNumber", "Suggest" },
                values: new object[] { 3.0, "Recommend" });

            migrationBuilder.UpdateData(
                table: "Packages",
                keyColumn: "Id",
                keyValue: new Guid("daa6fc87-6461-49d4-b3a5-c9e4cc30bc59"),
                columns: new[] { "MonthBonusNumber", "Suggest" },
                values: new object[] { 1.5, "BestSeller" });
        }
    }
}
