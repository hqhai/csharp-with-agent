using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateGradingClassForumResultTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CheckCsoId",
                table: "ClassForumResults",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CheckStartDate",
                table: "ClassForumResults",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "GradingStartDate",
                table: "ClassForumResults",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CheckCsoId",
                table: "ClassForumResults");

            migrationBuilder.DropColumn(
                name: "CheckStartDate",
                table: "ClassForumResults");

            migrationBuilder.DropColumn(
                name: "GradingStartDate",
                table: "ClassForumResults");
        }
    }
}
