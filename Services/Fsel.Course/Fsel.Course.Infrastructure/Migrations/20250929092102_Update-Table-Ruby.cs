using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTableRuby : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Position",
                table: "RubyAnnotations");

            migrationBuilder.DropColumn(
                name: "PrefixContext",
                table: "RubyAnnotations");

            migrationBuilder.DropColumn(
                name: "SuffixContext",
                table: "RubyAnnotations");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Position",
                table: "RubyAnnotations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PrefixContext",
                table: "RubyAnnotations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SuffixContext",
                table: "RubyAnnotations",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
