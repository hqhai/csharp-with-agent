using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Ordering.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_PackageEvent_Update_Field_Suggest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Suggest",
                table: "PackageEvents");

            migrationBuilder.AddColumn<string>(
                name: "SuggestStr",
                table: "PackageEvents",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "PackageEvents",
                keyColumn: "Id",
                keyValue: new Guid("2496f21a-7e62-41a3-a294-89674a411e03"),
                column: "SuggestStr",
                value: "[\"BestSeller\"]");

            migrationBuilder.UpdateData(
                table: "PackageEvents",
                keyColumn: "Id",
                keyValue: new Guid("7ec4ded7-afd4-4c84-8b71-12c8d89afabd"),
                column: "SuggestStr",
                value: "null");

            migrationBuilder.UpdateData(
                table: "PackageEvents",
                keyColumn: "Id",
                keyValue: new Guid("ebe09800-afd8-4621-a490-7f1fecf0e54c"),
                column: "SuggestStr",
                value: "[\"Recommend\"]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SuggestStr",
                table: "PackageEvents");

            migrationBuilder.AddColumn<string>(
                name: "Suggest",
                table: "PackageEvents",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "PackageEvents",
                keyColumn: "Id",
                keyValue: new Guid("2496f21a-7e62-41a3-a294-89674a411e03"),
                column: "Suggest",
                value: "BestSeller");

            migrationBuilder.UpdateData(
                table: "PackageEvents",
                keyColumn: "Id",
                keyValue: new Guid("7ec4ded7-afd4-4c84-8b71-12c8d89afabd"),
                column: "Suggest",
                value: null);

            migrationBuilder.UpdateData(
                table: "PackageEvents",
                keyColumn: "Id",
                keyValue: new Guid("ebe09800-afd8-4621-a490-7f1fecf0e54c"),
                column: "Suggest",
                value: "Recommend");
        }
    }
}
