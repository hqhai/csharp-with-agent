using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Identity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEventRegistration_IsSchoolarShip_AddColumn_StudentCodeAndStudentMainMajor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsSchoolarshipAdvising",
                table: "EventRegistrations",
                newName: "IsBussinessCheckBox");

            migrationBuilder.AddColumn<string>(
                name: "StudentCode",
                table: "EventRegistrations",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StudentMainMajor",
                table: "EventRegistrations",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StudentCode",
                table: "EventRegistrations");

            migrationBuilder.DropColumn(
                name: "StudentMainMajor",
                table: "EventRegistrations");

            migrationBuilder.RenameColumn(
                name: "IsBussinessCheckBox",
                table: "EventRegistrations",
                newName: "IsSchoolarshipAdvising");
        }
    }
}
