using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.System.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTokenConfigConfigTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "TokenConfigs",
                keyColumn: "Id",
                keyValue: new Guid("e5361e72-412e-4022-bc66-c813c56f1b10"),
                column: "Mission",
                value: "ReviewPlatform");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "TokenConfigs",
                keyColumn: "Id",
                keyValue: new Guid("e5361e72-412e-4022-bc66-c813c56f1b10"),
                column: "Mission",
                value: "ReviewFSEL");
        }
    }
}
