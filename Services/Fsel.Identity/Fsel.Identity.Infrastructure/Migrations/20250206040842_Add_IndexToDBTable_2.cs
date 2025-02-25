using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Identity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_IndexToDBTable_2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StudentCompetitionEvents_CompetitionEventId",
                table: "StudentCompetitionEvents");

            migrationBuilder.CreateIndex(
                name: "IX_UserOtpCodes_IsDeleted_OTPCode_Status",
                table: "UserOtpCodes",
                columns: new[] { "IsDeleted", "OTPCode", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Students_IsDeleted",
                table: "Students",
                column: "IsDeleted")
                .Annotation("SqlServer:Include", new[] { "CreatedDate", "School", "CourseLevel", "HumanId", "SchoolId" });

            migrationBuilder.CreateIndex(
                name: "IX_Students_IsDeleted_SchoolId",
                table: "Students",
                columns: new[] { "IsDeleted", "SchoolId" });

            migrationBuilder.CreateIndex(
                name: "IX_StudentCompetitionEvents_CompetitionEventId",
                table: "StudentCompetitionEvents",
                column: "CompetitionEventId")
                .Annotation("SqlServer:Include", new[] { "StudentId" });

            migrationBuilder.CreateIndex(
                name: "IX_StudentCompetitionEvents_StudentId",
                table: "StudentCompetitionEvents",
                column: "StudentId")
                .Annotation("SqlServer:Include", new[] { "CompetitionEventId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserOtpCodes_IsDeleted_OTPCode_Status",
                table: "UserOtpCodes");

            migrationBuilder.DropIndex(
                name: "IX_Students_IsDeleted",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Students_IsDeleted_SchoolId",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_StudentCompetitionEvents_CompetitionEventId",
                table: "StudentCompetitionEvents");

            migrationBuilder.DropIndex(
                name: "IX_StudentCompetitionEvents_StudentId",
                table: "StudentCompetitionEvents");

            migrationBuilder.CreateIndex(
                name: "IX_StudentCompetitionEvents_CompetitionEventId",
                table: "StudentCompetitionEvents",
                column: "CompetitionEventId");
        }
    }
}
