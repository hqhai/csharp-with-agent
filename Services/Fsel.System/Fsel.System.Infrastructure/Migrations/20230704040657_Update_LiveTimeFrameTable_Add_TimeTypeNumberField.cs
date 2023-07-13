using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.System.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_LiveTimeFrameTable_Add_TimeTypeNumberField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "StartTimeValue",
                table: "LiveTimeFrames",
                newName: "StartTime");

            migrationBuilder.RenameColumn(
                name: "EndTimeValue",
                table: "LiveTimeFrames",
                newName: "EndTime");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "StartTime",
                table: "LiveTimeFrames",
                newName: "StartTimeValue");

            migrationBuilder.RenameColumn(
                name: "EndTime",
                table: "LiveTimeFrames",
                newName: "EndTimeValue");
        }
    }
}
