using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateHomeWorkTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InstructionContent",
                table: "HomeWorks");

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "HomeWorks",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Code",
                table: "HomeWorks");

            migrationBuilder.AddColumn<string>(
                name: "InstructionContent",
                table: "HomeWorks",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
