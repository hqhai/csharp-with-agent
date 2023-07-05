using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.System.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_LiveTimeFrameTable_Remove_TimeField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "LiveTimeFrames");

            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "LiveTimeFrames");

            migrationBuilder.AddColumn<double>(
                name: "EndTimeValue",
                table: "LiveTimeFrames",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "StartTimeValue",
                table: "LiveTimeFrames",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndTimeValue",
                table: "LiveTimeFrames");

            migrationBuilder.DropColumn(
                name: "StartTimeValue",
                table: "LiveTimeFrames");

            migrationBuilder.AddColumn<DateTime>(
                name: "EndTime",
                table: "LiveTimeFrames",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "StartTime",
                table: "LiveTimeFrames",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
