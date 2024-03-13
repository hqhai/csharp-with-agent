using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Identity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateLocationStudentTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DistrictId",
                table: "Students",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProvinceId",
                table: "Students",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SchoolId",
                table: "Students",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DistrictId",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "ProvinceId",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "SchoolId",
                table: "Students");
        }
    }
}
