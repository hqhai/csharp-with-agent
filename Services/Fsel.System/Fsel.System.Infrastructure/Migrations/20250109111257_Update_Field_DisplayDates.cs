using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.System.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_Field_DisplayDates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DisplayEndDate",
                table: "Banners");

            migrationBuilder.DropColumn(
                name: "DisplayStartDate",
                table: "Banners");

            migrationBuilder.AddColumn<string>(
                name: "DisplayDateStr",
                table: "Banners",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DisplayDateStr",
                table: "Banners");

            migrationBuilder.AddColumn<DateTime>(
                name: "DisplayEndDate",
                table: "Banners",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DisplayStartDate",
                table: "Banners",
                type: "datetime2",
                nullable: true);
        }
    }
}
