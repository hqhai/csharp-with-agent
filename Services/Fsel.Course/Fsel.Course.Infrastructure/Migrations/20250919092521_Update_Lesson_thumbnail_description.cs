using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_Lesson_thumbnail_description : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Thurmbnail",
                table: "Lessons",
                newName: "Thumbnail");

            migrationBuilder.RenameColumn(
                name: "Desctiption",
                table: "Lessons",
                newName: "Description");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Thumbnail",
                table: "Lessons",
                newName: "Thurmbnail");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Lessons",
                newName: "Desctiption");
        }
    }
}
