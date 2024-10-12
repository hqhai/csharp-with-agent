using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Identity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AlterTableStudentRanking_DropColumnCompetitionEventId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudentRankingEvents_CompetitionEvents_CompetitionEventId",
                table: "StudentRankingEvents");

            migrationBuilder.DropIndex(
                name: "IX_StudentRankingEvents_CompetitionEventId",
                table: "StudentRankingEvents");

            migrationBuilder.DropColumn(
                name: "CompetitionEventId",
                table: "StudentRankingEvents");

            migrationBuilder.RenameColumn(
                name: "Proccess",
                table: "StudentRankingEvents",
                newName: "Process");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Process",
                table: "StudentRankingEvents",
                newName: "Proccess");

            migrationBuilder.AddColumn<Guid>(
                name: "CompetitionEventId",
                table: "StudentRankingEvents",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentRankingEvents_CompetitionEventId",
                table: "StudentRankingEvents",
                column: "CompetitionEventId");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentRankingEvents_CompetitionEvents_CompetitionEventId",
                table: "StudentRankingEvents",
                column: "CompetitionEventId",
                principalTable: "CompetitionEvents",
                principalColumn: "Id");
        }
    }
}
