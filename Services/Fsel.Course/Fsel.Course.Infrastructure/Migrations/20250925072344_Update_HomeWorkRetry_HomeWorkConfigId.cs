using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_HomeWorkRetry_HomeWorkConfigId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "HomeWorkConfigId",
                table: "HomeWorkRetries",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_HomeWorkRetries_HomeWorkConfigId",
                table: "HomeWorkRetries",
                column: "HomeWorkConfigId");

            migrationBuilder.AddForeignKey(
                name: "FK_HomeWorkRetries_HomeWorkConfigs_HomeWorkConfigId",
                table: "HomeWorkRetries",
                column: "HomeWorkConfigId",
                principalTable: "HomeWorkConfigs",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HomeWorkRetries_HomeWorkConfigs_HomeWorkConfigId",
                table: "HomeWorkRetries");

            migrationBuilder.DropIndex(
                name: "IX_HomeWorkRetries_HomeWorkConfigId",
                table: "HomeWorkRetries");

            migrationBuilder.DropColumn(
                name: "HomeWorkConfigId",
                table: "HomeWorkRetries");
        }
    }
}
