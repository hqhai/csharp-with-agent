using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Ordering.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_PackageTable_Revision_Field_Code : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Packages",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Packages",
                keyColumn: "Id",
                keyValue: new Guid("42d7ddb2-9f36-4f86-badc-67dc16bb722b"),
                columns: new[] { "DescriptionStr", "Name" },
                values: new object[] { "", "Fsel_3_Months" });

            migrationBuilder.UpdateData(
                table: "Packages",
                keyColumn: "Id",
                keyValue: new Guid("d13ee4ab-785a-425c-bd70-b74b61df42eb"),
                columns: new[] { "Code", "DescriptionStr", "Name" },
                values: new object[] { "BASIC", "", "Fsel_12_Months" });

            migrationBuilder.UpdateData(
                table: "Packages",
                keyColumn: "Id",
                keyValue: new Guid("daa6fc87-6461-49d4-b3a5-c9e4cc30bc59"),
                columns: new[] { "Code", "DescriptionStr", "Name" },
                values: new object[] { "BASIC", "", "Fsel_6_Months" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "Packages");

            migrationBuilder.UpdateData(
                table: "Packages",
                keyColumn: "Id",
                keyValue: new Guid("42d7ddb2-9f36-4f86-badc-67dc16bb722b"),
                column: "DescriptionStr",
                value: "[{\"content\":\"B\\u00E0i gi\\u1EA3ng , b\\u00E0i t\\u1EADp t\\u00EAn n\\u1EC1n t\\u1EA3ng E-learning\",\"status\":true},{\"content\":\"Truy c\\u1EADp b\\u00E0i t\\u1EADp h\\u01B0\\u1EDBng d\\u1EABn, v\\u00E0 b\\u00E0i thi Unit\",\"status\":true},{\"content\":\"Di\\u1EC5n \\u0111\\u00E0n\",\"status\":true},{\"content\":\"Gi\\u1EA3ng vi\\u00EAn nh\\u1EADn x\\u00E9t\",\"status\":false},{\"content\":\"Truy c\\u1EADp ti\\u1EBFt h\\u1ECDc tr\\u1EF1c tuy\\u1EBFn cho k\\u1EF9 n\\u0103ng n\\u00F3i v\\u1EDBi Gi\\u1EA3ng vi\\u00EAn\",\"status\":false}]");

            migrationBuilder.UpdateData(
                table: "Packages",
                keyColumn: "Id",
                keyValue: new Guid("d13ee4ab-785a-425c-bd70-b74b61df42eb"),
                columns: new[] { "Code", "DescriptionStr" },
                values: new object[] { "PREMIUM", "[{\"content\":\"B\\u00E0i gi\\u1EA3ng , b\\u00E0i t\\u1EADp t\\u00EAn n\\u1EC1n t\\u1EA3ng E-learning\",\"status\":true},{\"content\":\"Truy c\\u1EADp b\\u00E0i t\\u1EADp h\\u01B0\\u1EDBng d\\u1EABn, v\\u00E0 b\\u00E0i thi Unit\",\"status\":true},{\"content\":\"Di\\u1EC5n \\u0111\\u00E0n\",\"status\":true},{\"content\":\"Gi\\u1EA3ng vi\\u00EAn nh\\u1EADn x\\u00E9t\",\"status\":true},{\"content\":\"Truy c\\u1EADp ti\\u1EBFt h\\u1ECDc tr\\u1EF1c tuy\\u1EBFn cho k\\u1EF9 n\\u0103ng n\\u00F3i v\\u1EDBi Gi\\u1EA3ng vi\\u00EAn\",\"status\":true}]" });

            migrationBuilder.UpdateData(
                table: "Packages",
                keyColumn: "Id",
                keyValue: new Guid("daa6fc87-6461-49d4-b3a5-c9e4cc30bc59"),
                columns: new[] { "Code", "DescriptionStr" },
                values: new object[] { "STANDARD", "[{\"content\":\"B\\u00E0i gi\\u1EA3ng , b\\u00E0i t\\u1EADp t\\u00EAn n\\u1EC1n t\\u1EA3ng E-learning\",\"status\":true},{\"content\":\"Truy c\\u1EADp b\\u00E0i t\\u1EADp h\\u01B0\\u1EDBng d\\u1EABn, v\\u00E0 b\\u00E0i thi Unit\",\"status\":true},{\"content\":\"Di\\u1EC5n \\u0111\\u00E0n\",\"status\":true},{\"content\":\"Gi\\u1EA3ng vi\\u00EAn nh\\u1EADn x\\u00E9t\",\"status\":true},{\"content\":\"Truy c\\u1EADp ti\\u1EBFt h\\u1ECDc tr\\u1EF1c tuy\\u1EBFn cho k\\u1EF9 n\\u0103ng n\\u00F3i v\\u1EDBi Gi\\u1EA3ng vi\\u00EAn\",\"status\":false}]" });
        }
    }
}
