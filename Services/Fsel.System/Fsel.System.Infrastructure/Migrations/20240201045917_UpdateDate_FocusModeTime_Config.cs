using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.System.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDate_FocusModeTime_Config : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "FocusTimeConfigs",
                keyColumn: "Id",
                keyValue: new Guid("0a7b57f3-c964-4f1b-8986-df1579c5d08b"),
                column: "TargetTime",
                value: 900.0);

            migrationBuilder.UpdateData(
                table: "FocusTimeConfigs",
                keyColumn: "Id",
                keyValue: new Guid("124f4341-4c87-4e5f-ba4d-2481d8d36737"),
                column: "TargetTime",
                value: 2700.0);

            migrationBuilder.UpdateData(
                table: "FocusTimeConfigs",
                keyColumn: "Id",
                keyValue: new Guid("82061293-c9d0-4598-99f1-8dfd8162b999"),
                column: "TargetTime",
                value: 3600.0);

            migrationBuilder.UpdateData(
                table: "FocusTimeConfigs",
                keyColumn: "Id",
                keyValue: new Guid("9b4fa7b6-1af4-458b-82d9-621c1a88654a"),
                column: "TargetTime",
                value: 1800.0);

            migrationBuilder.UpdateData(
                table: "FocusTimeConfigs",
                keyColumn: "Id",
                keyValue: new Guid("fd7e66d3-a29b-4da7-bb29-2283536d836a"),
                column: "TargetTime",
                value: 5400.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "FocusTimeConfigs",
                keyColumn: "Id",
                keyValue: new Guid("0a7b57f3-c964-4f1b-8986-df1579c5d08b"),
                column: "TargetTime",
                value: 1800.0);

            migrationBuilder.UpdateData(
                table: "FocusTimeConfigs",
                keyColumn: "Id",
                keyValue: new Guid("124f4341-4c87-4e5f-ba4d-2481d8d36737"),
                column: "TargetTime",
                value: 5400.0);

            migrationBuilder.UpdateData(
                table: "FocusTimeConfigs",
                keyColumn: "Id",
                keyValue: new Guid("82061293-c9d0-4598-99f1-8dfd8162b999"),
                column: "TargetTime",
                value: 7200.0);

            migrationBuilder.UpdateData(
                table: "FocusTimeConfigs",
                keyColumn: "Id",
                keyValue: new Guid("9b4fa7b6-1af4-458b-82d9-621c1a88654a"),
                column: "TargetTime",
                value: 3600.0);

            migrationBuilder.UpdateData(
                table: "FocusTimeConfigs",
                keyColumn: "Id",
                keyValue: new Guid("fd7e66d3-a29b-4da7-bb29-2283536d836a"),
                column: "TargetTime",
                value: 10800.0);
        }
    }
}
