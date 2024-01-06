using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_VideoTimeCodeResult_RetryWorkingTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RemainingTime",
                table: "VideoTimeCodeResults",
                newName: "WorkingTime");

            migrationBuilder.AddColumn<double>(
                name: "RetryWorkingTime",
                table: "VideoTimeCodeResults",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RetryWorkingTime",
                table: "VideoTimeCodeResults");

            migrationBuilder.RenameColumn(
                name: "WorkingTime",
                table: "VideoTimeCodeResults",
                newName: "RemainingTime");
        }
    }
}
