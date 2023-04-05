using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateVideoResult : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Feedback",
                table: "VideoResults",
                type: "nvarchar(max)",
                maxLength: 100000000,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "NumberOfStars",
                table: "VideoResults",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Feedback",
                table: "VideoResults");

            migrationBuilder.DropColumn(
                name: "NumberOfStars",
                table: "VideoResults");
        }
    }
}
