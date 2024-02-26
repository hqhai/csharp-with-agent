using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Fsel.Ordering.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_PackageTable_Revision_Field_Code : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Packages",
                keyColumn: "Id",
                keyValue: new Guid("42d7ddb2-9f36-4f86-badc-67dc16bb722b"));

            migrationBuilder.DeleteData(
                table: "Packages",
                keyColumn: "Id",
                keyValue: new Guid("d13ee4ab-785a-425c-bd70-b74b61df42eb"));

            migrationBuilder.DeleteData(
                table: "Packages",
                keyColumn: "Id",
                keyValue: new Guid("daa6fc87-6461-49d4-b3a5-c9e4cc30bc59"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Packages",
                columns: new[] { "Id", "Code", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "DescriptionStr", "IsDeleted", "MonthNumber", "Price", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[,]
                {
                    { new Guid("42d7ddb2-9f36-4f86-badc-67dc16bb722b"), "BASIC", new DateTime(2023, 7, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "[{\"content\":\"B\\u00E0i gi\\u1EA3ng , b\\u00E0i t\\u1EADp t\\u00EAn n\\u1EC1n t\\u1EA3ng E-learning\",\"status\":true},{\"content\":\"Truy c\\u1EADp b\\u00E0i t\\u1EADp h\\u01B0\\u1EDBng d\\u1EABn, v\\u00E0 b\\u00E0i thi Unit\",\"status\":true},{\"content\":\"Di\\u1EC5n \\u0111\\u00E0n\",\"status\":true},{\"content\":\"Gi\\u1EA3ng vi\\u00EAn nh\\u1EADn x\\u00E9t\",\"status\":false},{\"content\":\"Truy c\\u1EADp ti\\u1EBFt h\\u1ECDc tr\\u1EF1c tuy\\u1EBFn cho k\\u1EF9 n\\u0103ng n\\u00F3i v\\u1EDBi Gi\\u1EA3ng vi\\u00EAn\",\"status\":false}]", false, 3, 1000000m, null, null, null },
                    { new Guid("d13ee4ab-785a-425c-bd70-b74b61df42eb"), "PREMIUM", new DateTime(2023, 7, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "[{\"content\":\"B\\u00E0i gi\\u1EA3ng , b\\u00E0i t\\u1EADp t\\u00EAn n\\u1EC1n t\\u1EA3ng E-learning\",\"status\":true},{\"content\":\"Truy c\\u1EADp b\\u00E0i t\\u1EADp h\\u01B0\\u1EDBng d\\u1EABn, v\\u00E0 b\\u00E0i thi Unit\",\"status\":true},{\"content\":\"Di\\u1EC5n \\u0111\\u00E0n\",\"status\":true},{\"content\":\"Gi\\u1EA3ng vi\\u00EAn nh\\u1EADn x\\u00E9t\",\"status\":true},{\"content\":\"Truy c\\u1EADp ti\\u1EBFt h\\u1ECDc tr\\u1EF1c tuy\\u1EBFn cho k\\u1EF9 n\\u0103ng n\\u00F3i v\\u1EDBi Gi\\u1EA3ng vi\\u00EAn\",\"status\":true}]", false, 12, 10000000m, null, null, null },
                    { new Guid("daa6fc87-6461-49d4-b3a5-c9e4cc30bc59"), "STANDARD", new DateTime(2023, 7, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "[{\"content\":\"B\\u00E0i gi\\u1EA3ng , b\\u00E0i t\\u1EADp t\\u00EAn n\\u1EC1n t\\u1EA3ng E-learning\",\"status\":true},{\"content\":\"Truy c\\u1EADp b\\u00E0i t\\u1EADp h\\u01B0\\u1EDBng d\\u1EABn, v\\u00E0 b\\u00E0i thi Unit\",\"status\":true},{\"content\":\"Di\\u1EC5n \\u0111\\u00E0n\",\"status\":true},{\"content\":\"Gi\\u1EA3ng vi\\u00EAn nh\\u1EADn x\\u00E9t\",\"status\":true},{\"content\":\"Truy c\\u1EADp ti\\u1EBFt h\\u1ECDc tr\\u1EF1c tuy\\u1EBFn cho k\\u1EF9 n\\u0103ng n\\u00F3i v\\u1EDBi Gi\\u1EA3ng vi\\u00EAn\",\"status\":false}]", false, 6, 3000000m, null, null, null }
                });
        }
    }
}
