using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Ordering.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_PackageTable_EditData_1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Packages",
                keyColumn: "Id",
                keyValue: new Guid("42d7ddb2-9f36-4f86-badc-67dc16bb722b"),
                columns: new[] { "DescriptionStr", "MonthNumber", "Name", "Price" },
                values: new object[] { "[{\"content\":\"Giá trên đã bao gồm Chi phí chính sách Cambridge: 150,000 VND\",\"status\":true}]", 1, "Fsel_1_Month", 650000m });

            migrationBuilder.UpdateData(
                table: "Packages",
                keyColumn: "Id",
                keyValue: new Guid("d13ee4ab-785a-425c-bd70-b74b61df42eb"),
                columns: new[] { "DescriptionStr", "Price" },
                values: new object[] { "[{\"content\":\"Giá trên đã bao gồm Chi phí chính sách Cambridge: 150,000 VND\",\"status\":true}]", 3100000m });

            migrationBuilder.UpdateData(
                table: "Packages",
                keyColumn: "Id",
                keyValue: new Guid("daa6fc87-6461-49d4-b3a5-c9e4cc30bc59"),
                columns: new[] { "DescriptionStr", "Price" },
                values: new object[] { "[{\"content\":\"Giá trên đã bao gồm Chi phí chính sách Cambridge: 150,000 VND\",\"status\":true}]", 2100000m });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Packages",
                keyColumn: "Id",
                keyValue: new Guid("42d7ddb2-9f36-4f86-badc-67dc16bb722b"),
                columns: new[] { "DescriptionStr", "MonthNumber", "Name", "Price" },
                values: new object[] { "", 3, "Fsel_3_Months", 3000000m });

            migrationBuilder.UpdateData(
                table: "Packages",
                keyColumn: "Id",
                keyValue: new Guid("d13ee4ab-785a-425c-bd70-b74b61df42eb"),
                columns: new[] { "DescriptionStr", "Price" },
                values: new object[] { "", 8000000m });

            migrationBuilder.UpdateData(
                table: "Packages",
                keyColumn: "Id",
                keyValue: new Guid("daa6fc87-6461-49d4-b3a5-c9e4cc30bc59"),
                columns: new[] { "DescriptionStr", "Price" },
                values: new object[] { "", 5000000m });
        }
    }
}
