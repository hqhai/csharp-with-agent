using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Identity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_StudentTable_Add_Validate_Token : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
            UPDATE Students
            SET NumberOfToken = 0
            WHERE NumberOfToken < 0;
            ");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Student_NumberOfToken_NonNegative",
                table: "Students",
                sql: "[NumberOfToken] >= 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Student_NumberOfToken_NonNegative",
                table: "Students");
        }
    }
}
