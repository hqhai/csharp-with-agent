using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateExtraPracticeBook : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FilePathsStr",
                table: "ExtraPractices");

            migrationBuilder.AddColumn<string>(
                name: "BookBackgroundPath",
                table: "ExtraPractices",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BookCoverPath",
                table: "ExtraPractices",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BookFilePath",
                table: "ExtraPractices",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BookBackgroundPath",
                table: "ExtraPractices");

            migrationBuilder.DropColumn(
                name: "BookCoverPath",
                table: "ExtraPractices");

            migrationBuilder.DropColumn(
                name: "BookFilePath",
                table: "ExtraPractices");

            migrationBuilder.AddColumn<string>(
                name: "FilePathsStr",
                table: "ExtraPractices",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
