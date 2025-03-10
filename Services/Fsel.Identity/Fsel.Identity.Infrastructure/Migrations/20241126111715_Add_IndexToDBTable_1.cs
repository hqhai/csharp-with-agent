using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Identity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_IndexToDBTable_1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_StudentFocusTimes_IsDeleted_StudentId_CreatedDate",
                table: "StudentFocusTimes",
                columns: new[] { "IsDeleted", "StudentId", "CreatedDate" });

            migrationBuilder.CreateIndex(
                name: "IX_StudentFocusTimes_IsDeleted_StudentId_CreatedDate_TargetTime",
                table: "StudentFocusTimes",
                columns: new[] { "IsDeleted", "StudentId", "CreatedDate", "TargetTime" });

            migrationBuilder.CreateIndex(
                name: "IX_StudentDailyStreak_IsDeleted_StudentId",
                table: "StudentDailyStreak",
                columns: new[] { "IsDeleted", "StudentId" });

            migrationBuilder.CreateIndex(
                name: "IX_StudentCompetitionEvents_IsDeleted_StudentId",
                table: "StudentCompetitionEvents",
                columns: new[] { "IsDeleted", "StudentId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StudentFocusTimes_IsDeleted_StudentId_CreatedDate",
                table: "StudentFocusTimes");

            migrationBuilder.DropIndex(
                name: "IX_StudentFocusTimes_IsDeleted_StudentId_CreatedDate_TargetTime",
                table: "StudentFocusTimes");

            migrationBuilder.DropIndex(
                name: "IX_StudentDailyStreak_IsDeleted_StudentId",
                table: "StudentDailyStreak");

            migrationBuilder.DropIndex(
                name: "IX_StudentCompetitionEvents_IsDeleted_StudentId",
                table: "StudentCompetitionEvents");
        }
    }
}
