using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.System.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_CreatedDateLocal_Into_DailyQuiz : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDateLocal",
                table: "DailyQuizWinners",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDateLocal",
                table: "DailyQuizHistories",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedDateLocal",
                table: "DailyQuizWinners");

            migrationBuilder.DropColumn(
                name: "CreatedDateLocal",
                table: "DailyQuizHistories");
        }
    }
}
