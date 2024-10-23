using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Identity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTableCompetitionEvent_AddFieldAndRename : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudentRankingEvents_CompetitionEvents_CompetitionRankingId",
                table: "StudentRankingEvents");

            migrationBuilder.RenameColumn(
                name: "CompetitionRankingId",
                table: "StudentRankingEvents",
                newName: "CompetitionEventId");

            migrationBuilder.RenameIndex(
                name: "IX_StudentRankingEvents_CompetitionRankingId",
                table: "StudentRankingEvents",
                newName: "IX_StudentRankingEvents_CompetitionEventId");

            migrationBuilder.AddColumn<string>(
                name: "SchoolIdsStr",
                table: "CompetitionEvents",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentRankingEvents_CompetitionEvents_CompetitionEventId",
                table: "StudentRankingEvents",
                column: "CompetitionEventId",
                principalTable: "CompetitionEvents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudentRankingEvents_CompetitionEvents_CompetitionEventId",
                table: "StudentRankingEvents");

            migrationBuilder.DropColumn(
                name: "SchoolIdsStr",
                table: "CompetitionEvents");

            migrationBuilder.RenameColumn(
                name: "CompetitionEventId",
                table: "StudentRankingEvents",
                newName: "CompetitionRankingId");

            migrationBuilder.RenameIndex(
                name: "IX_StudentRankingEvents_CompetitionEventId",
                table: "StudentRankingEvents",
                newName: "IX_StudentRankingEvents_CompetitionRankingId");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentRankingEvents_CompetitionEvents_CompetitionRankingId",
                table: "StudentRankingEvents",
                column: "CompetitionRankingId",
                principalTable: "CompetitionEvents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
