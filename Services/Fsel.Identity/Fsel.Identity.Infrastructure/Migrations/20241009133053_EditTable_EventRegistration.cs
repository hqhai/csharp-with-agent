using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Identity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EditTable_EventRegistration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSchoolarshipAdvising",
                table: "EventRegistrations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ParentEmail",
                table: "EventRegistrations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ParentPhoneNumber",
                table: "EventRegistrations",
                type: "nvarchar(12)",
                maxLength: 12,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TeacherPhoneNumber",
                table: "EventRegistrations",
                type: "nvarchar(12)",
                maxLength: 12,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSchoolarshipAdvising",
                table: "EventRegistrations");

            migrationBuilder.DropColumn(
                name: "ParentEmail",
                table: "EventRegistrations");

            migrationBuilder.DropColumn(
                name: "ParentPhoneNumber",
                table: "EventRegistrations");

            migrationBuilder.DropColumn(
                name: "TeacherPhoneNumber",
                table: "EventRegistrations");
        }
    }
}
