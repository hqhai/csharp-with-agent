using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Identity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTableStudentRanking_ChangeColumnType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "TotalScore",
                table: "StudentRankings",
                type: "float",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.UpdateData(
                table: "Platform",
                keyColumn: "Id",
                keyValue: new Guid("856818e6-9e20-43d8-964f-5dab2ba1a355"),
                column: "CreatedDate",
                value: new DateTime(2023, 10, 7, 11, 22, 45, 955, DateTimeKind.Local).AddTicks(333));

            migrationBuilder.UpdateData(
                table: "Platform",
                keyColumn: "Id",
                keyValue: new Guid("c9c55ef5-01e0-4fa6-b68a-3aaea1089548"),
                column: "CreatedDate",
                value: new DateTime(2023, 10, 7, 11, 22, 45, 954, DateTimeKind.Local).AddTicks(9863));

            migrationBuilder.UpdateData(
                table: "Platform",
                keyColumn: "Id",
                keyValue: new Guid("f144a094-3f49-4ca8-8f1e-7234289dd1a5"),
                column: "CreatedDate",
                value: new DateTime(2023, 10, 7, 11, 22, 45, 955, DateTimeKind.Local).AddTicks(370));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "TotalScore",
                table: "StudentRankings",
                type: "int",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.UpdateData(
                table: "Platform",
                keyColumn: "Id",
                keyValue: new Guid("856818e6-9e20-43d8-964f-5dab2ba1a355"),
                column: "CreatedDate",
                value: new DateTime(2023, 10, 6, 16, 16, 6, 665, DateTimeKind.Local).AddTicks(707));

            migrationBuilder.UpdateData(
                table: "Platform",
                keyColumn: "Id",
                keyValue: new Guid("c9c55ef5-01e0-4fa6-b68a-3aaea1089548"),
                column: "CreatedDate",
                value: new DateTime(2023, 10, 6, 16, 16, 6, 665, DateTimeKind.Local).AddTicks(443));

            migrationBuilder.UpdateData(
                table: "Platform",
                keyColumn: "Id",
                keyValue: new Guid("f144a094-3f49-4ca8-8f1e-7234289dd1a5"),
                column: "CreatedDate",
                value: new DateTime(2023, 10, 6, 16, 16, 6, 665, DateTimeKind.Local).AddTicks(733));
        }
    }
}
