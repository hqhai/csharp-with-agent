using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_Field_IsArchive_To_Tables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "FinalTests",
                newName: "IsArchive");

            migrationBuilder.AddColumn<bool>(
                name: "IsArchive",
                table: "Videos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchive",
                table: "Units",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchive",
                table: "PlacementTests",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchive",
                table: "MockTests",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchive",
                table: "Lessons",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchive",
                table: "HomeWorks",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchive",
                table: "ExtraPractices",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchive",
                table: "Courses",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsArchive",
                table: "Videos");

            migrationBuilder.DropColumn(
                name: "IsArchive",
                table: "Units");

            migrationBuilder.DropColumn(
                name: "IsArchive",
                table: "PlacementTests");

            migrationBuilder.DropColumn(
                name: "IsArchive",
                table: "MockTests");

            migrationBuilder.DropColumn(
                name: "IsArchive",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "IsArchive",
                table: "HomeWorks");

            migrationBuilder.DropColumn(
                name: "IsArchive",
                table: "ExtraPractices");

            migrationBuilder.DropColumn(
                name: "IsArchive",
                table: "Courses");

            migrationBuilder.RenameColumn(
                name: "IsArchive",
                table: "FinalTests",
                newName: "IsActive");
        }
    }
}
