using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Ordering.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_VoucherTable_Add_And_Update_Fields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApplicableSubjectsStr",
                table: "Vouchers",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(
                "UPDATE Vouchers " +
                "SET ApplicableSubjectsStr = " +
                "CASE " +
                "WHEN VoucherType = 'All' THEN '[\"NewSale\",\"CurrentStudent\",\"Alumni\"]' " +
                "ELSE '[\"' + VoucherType + '\"]' " +
                "END"
            );

            migrationBuilder.Sql(
                "Update Vouchers " +
                "Set Source = " +
                "Case " +
                "When Source = 'Retail' or Source = 'MasterAgency'  Then 'Auto' " +
                "ELSE 'Admin' " +
                "END"
            );

            migrationBuilder.DropColumn(
                name: "VoucherType",
                table: "Vouchers");

            migrationBuilder.RenameColumn(
                name: "Percent",
                table: "Vouchers",
                newName: "Value");

            migrationBuilder.AlterColumn<DateTime>(
                name: "EndDate",
                table: "Vouchers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Vouchers",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(8)",
                oldMaxLength: 8);

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Vouchers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(
                "Update Vouchers " +
                "Set Category = 'Percent'"
            );

            migrationBuilder.AddColumn<string>(
                name: "ApplicableEmailsStr",
                table: "Vouchers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Banner",
                table: "Vouchers",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CodePrefix",
                table: "Vouchers",
                type: "nvarchar(5)",
                maxLength: 5,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DescriptionStr",
                table: "Vouchers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EventIdsStr",
                table: "Vouchers",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(
                "UPDATE Vouchers " +
                "SET EventIdsStr = (" +
                "SELECT TOP(1) '[\"' + CAST(Id AS NVARCHAR(MAX)) + '\"]' " +
                "FROM Events " +
                "WHERE IsDefault = 1)"
            );

            migrationBuilder.AddColumn<bool>(
                name: "IsShowMyVoucher",
                table: "Vouchers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "NumberOfChanges",
                table: "Vouchers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TranslationsStr",
                table: "Vouchers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "VoucherType",
                table: "Vouchers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(
                "UPDATE Vouchers " +
                "SET VoucherType = CASE " +
                "WHEN ApplicableSubjectsStr = '[\"NewSale\",\"CurrentStudent\",\"Alumni\"]' THEN 'All' " +
                "ELSE JSON_VALUE(ApplicableSubjectsStr, '$[0]') " +
                "END " +
                "WHERE ISJSON(ApplicableSubjectsStr) = 1"
                );

            migrationBuilder.Sql(
                "Update Vouchers " +
                "Set Source = " +
                "Case " +
                "When Source = 'Auto' Then 'MasterAgency' " +
                "ELSE 'Admin' " +
                "END"
            );

            migrationBuilder.DropColumn(
               name: "ApplicableSubjectsStr",
               table: "Vouchers");

            migrationBuilder.DropColumn(
                name: "ApplicableEmailsStr",
                table: "Vouchers");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "Vouchers");

            migrationBuilder.DropColumn(
                name: "Banner",
                table: "Vouchers");

            migrationBuilder.DropColumn(
                name: "CodePrefix",
                table: "Vouchers");

            migrationBuilder.DropColumn(
                name: "DescriptionStr",
                table: "Vouchers");

            migrationBuilder.DropColumn(
                name: "EventIdsStr",
                table: "Vouchers");

            migrationBuilder.DropColumn(
                name: "IsShowMyVoucher",
                table: "Vouchers");

            migrationBuilder.DropColumn(
                name: "NumberOfChanges",
                table: "Vouchers");

            migrationBuilder.DropColumn(
                name: "TranslationsStr",
                table: "Vouchers");

            migrationBuilder.RenameColumn(
                name: "Value",
                table: "Vouchers",
                newName: "Percent");

            migrationBuilder.AlterColumn<DateTime>(
                name: "EndDate",
                table: "Vouchers",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Vouchers",
                type: "nvarchar(8)",
                maxLength: 8,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10);
        }
    }
}
