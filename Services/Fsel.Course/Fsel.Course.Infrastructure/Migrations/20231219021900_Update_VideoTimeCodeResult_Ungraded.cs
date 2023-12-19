using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_VideoTimeCodeResult_Ungraded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CorrectCountUngraded",
                table: "VideoTimeCodeResults",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CorrectTotalUngraded",
                table: "VideoTimeCodeResults",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SkillScoreUngradedStr",
                table: "VideoTimeCodeResults",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CorrectCountUngraded",
                table: "VideoTimeCodeResults");

            migrationBuilder.DropColumn(
                name: "CorrectTotalUngraded",
                table: "VideoTimeCodeResults");

            migrationBuilder.DropColumn(
                name: "SkillScoreUngradedStr",
                table: "VideoTimeCodeResults");
        }
    }
}
