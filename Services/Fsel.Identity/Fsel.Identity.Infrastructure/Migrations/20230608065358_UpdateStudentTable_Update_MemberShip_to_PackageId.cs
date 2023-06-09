using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Identity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateStudentTable_Update_MemberShip_to_PackageId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Membership",
                table: "Students");

            migrationBuilder.AddColumn<Guid>(
                name: "PackageId",
                table: "Students",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PackageId",
                table: "Students");

            migrationBuilder.AddColumn<string>(
                name: "Membership",
                table: "Students",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);
        }
    }
}
