using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_VideoTimeCodeAnswer_IsFirstSubmit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsFirstSubmit",
                table: "VideoTimeCodeAnswers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsFirstSubmit",
                table: "PlacementTestAnswers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsFirstSubmit",
                table: "MockTestAnswers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsFirstSubmit",
                table: "HomeWorkAnswers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsFirstSubmit",
                table: "FinalTestAnswers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsFirstSubmit",
                table: "ExtraPracticeAnswers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsFirstSubmit",
                table: "VideoTimeCodeAnswers");

            migrationBuilder.DropColumn(
                name: "IsFirstSubmit",
                table: "PlacementTestAnswers");

            migrationBuilder.DropColumn(
                name: "IsFirstSubmit",
                table: "MockTestAnswers");

            migrationBuilder.DropColumn(
                name: "IsFirstSubmit",
                table: "HomeWorkAnswers");

            migrationBuilder.DropColumn(
                name: "IsFirstSubmit",
                table: "FinalTestAnswers");

            migrationBuilder.DropColumn(
                name: "IsFirstSubmit",
                table: "ExtraPracticeAnswers");
        }
    }
}
