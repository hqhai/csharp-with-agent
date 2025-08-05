using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Identity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_New_Field_In_Table_UserSchools : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "UserSchools",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EventCode",
                table: "UserSchools",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LocalId",
                table: "UserSchools",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SchoolName",
                table: "UserSchools",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "City",
                table: "UserSchools");

            migrationBuilder.DropColumn(
                name: "EventCode",
                table: "UserSchools");

            migrationBuilder.DropColumn(
                name: "LocalId",
                table: "UserSchools");

            migrationBuilder.DropColumn(
                name: "SchoolName",
                table: "UserSchools");
        }
    }
}
