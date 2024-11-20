using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Fsel.Ordering.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_PackageTable_Add_Field_Status : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Packages",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Packages",
                keyColumn: "Id",
                keyValue: new Guid("42d7ddb2-9f36-4f86-badc-67dc16bb722b"),
                column: "Status",
                value: "InActive");

            migrationBuilder.UpdateData(
                table: "Packages",
                keyColumn: "Id",
                keyValue: new Guid("d13ee4ab-785a-425c-bd70-b74b61df42eb"),
                columns: new[] { "PriceMonth", "Status" },
                values: new object[] { 300000m, "Active" });

            migrationBuilder.UpdateData(
                table: "Packages",
                keyColumn: "Id",
                keyValue: new Guid("daa6fc87-6461-49d4-b3a5-c9e4cc30bc59"),
                columns: new[] { "PriceMonth", "Status" },
                values: new object[] { 400000m, "Active" });

            migrationBuilder.InsertData(
                table: "Packages",
                columns: new[] { "Id", "Code", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "IncentivesWhenPurchasing", "IsDeleted", "MonthNumber", "Name", "Price", "PriceMonth", "Status", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[] { new Guid("cbd0a22a-356d-47da-8b42-849a0121361c"), "PREMIUM", new DateTime(2024, 9, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Tặng phí bản quyền của NXB Đại học Cambridge", false, 24, "Fsel_24_Months", 7200000m, 300000m, "Active", null, null, null });

            migrationBuilder.InsertData(
                table: "PackageTranslations",
                columns: new[] { "Id", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "IncentivesWhenPurchasing", "IsDeleted", "Language", "PackageId", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[,]
                {
                    { new Guid("7d3ea25f-b36e-487c-9d3b-c1a2db1b03cb"), new DateTime(2024, 9, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Tặng phí bản quyền của NXB Đại học Cambridge", false, "vi-VN", new Guid("cbd0a22a-356d-47da-8b42-849a0121361c"), null, null, null },
                    { new Guid("8789802b-84a4-4276-873a-5a4c140cae2c"), new DateTime(2024, 9, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Faire don des droits d'auteur des Éditions de l'Université de Cambridge", false, "fr-FR", new Guid("cbd0a22a-356d-47da-8b42-849a0121361c"), null, null, null },
                    { new Guid("c9d07f5d-dfdb-448d-87c9-97a2a07b8237"), new DateTime(2024, 9, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Free copyright fee from Cambridge University Press", false, "en-US", new Guid("cbd0a22a-356d-47da-8b42-849a0121361c"), null, null, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "PackageTranslations",
                keyColumn: "Id",
                keyValue: new Guid("7d3ea25f-b36e-487c-9d3b-c1a2db1b03cb"));

            migrationBuilder.DeleteData(
                table: "PackageTranslations",
                keyColumn: "Id",
                keyValue: new Guid("8789802b-84a4-4276-873a-5a4c140cae2c"));

            migrationBuilder.DeleteData(
                table: "PackageTranslations",
                keyColumn: "Id",
                keyValue: new Guid("c9d07f5d-dfdb-448d-87c9-97a2a07b8237"));

            migrationBuilder.DeleteData(
                table: "Packages",
                keyColumn: "Id",
                keyValue: new Guid("cbd0a22a-356d-47da-8b42-849a0121361c"));

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Packages");

            migrationBuilder.UpdateData(
                table: "Packages",
                keyColumn: "Id",
                keyValue: new Guid("d13ee4ab-785a-425c-bd70-b74b61df42eb"),
                column: "PriceMonth",
                value: 240000m);

            migrationBuilder.UpdateData(
                table: "Packages",
                keyColumn: "Id",
                keyValue: new Guid("daa6fc87-6461-49d4-b3a5-c9e4cc30bc59"),
                column: "PriceMonth",
                value: 320000m);
        }
    }
}
