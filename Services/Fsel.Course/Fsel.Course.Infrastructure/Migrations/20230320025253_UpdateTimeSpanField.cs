using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTimeSpanField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ExecutionTimeTicks",
                table: "VideoTimeCodes",
                newName: "ExecutionTime");

            migrationBuilder.RenameColumn(
                name: "DisplayTimeTicks",
                table: "VideoTimeCodes",
                newName: "DisplayTime");

            migrationBuilder.RenameColumn(
                name: "TaggetTimeLimitTicks",
                table: "ClassForums",
                newName: "TaggetTimeLimit");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ExecutionTime",
                table: "VideoTimeCodes",
                newName: "ExecutionTimeTicks");

            migrationBuilder.RenameColumn(
                name: "DisplayTime",
                table: "VideoTimeCodes",
                newName: "DisplayTimeTicks");

            migrationBuilder.RenameColumn(
                name: "TaggetTimeLimit",
                table: "ClassForums",
                newName: "TaggetTimeLimitTicks");
        }
    }
}
