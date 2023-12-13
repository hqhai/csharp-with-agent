using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_Token_Result : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TokenDone",
                table: "VideoTimeCodeResults",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TokenHighestStreak",
                table: "VideoTimeCodeResults",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TokenQuestionReward",
                table: "VideoTimeCodeResults",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TokenSuperFire",
                table: "VideoTimeCodeResults",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TokenDone",
                table: "VideoResults",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TokenHighestStreak",
                table: "VideoResults",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TokenQuestionReward",
                table: "VideoResults",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TokenSuperFire",
                table: "VideoResults",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TokenDone",
                table: "SectionGroupResults",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TokenHighestStreak",
                table: "SectionGroupResults",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TokenSuperFire",
                table: "SectionGroupResults",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TokenDone",
                table: "MockTestResults",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TokenHighestStreak",
                table: "MockTestResults",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TokenSuperFire",
                table: "MockTestResults",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TokenDone",
                table: "FinalTestResults",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TokenHighestStreak",
                table: "FinalTestResults",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TokenSuperFire",
                table: "FinalTestResults",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TokenDone",
                table: "VideoTimeCodeResults");

            migrationBuilder.DropColumn(
                name: "TokenHighestStreak",
                table: "VideoTimeCodeResults");

            migrationBuilder.DropColumn(
                name: "TokenQuestionReward",
                table: "VideoTimeCodeResults");

            migrationBuilder.DropColumn(
                name: "TokenSuperFire",
                table: "VideoTimeCodeResults");

            migrationBuilder.DropColumn(
                name: "TokenDone",
                table: "VideoResults");

            migrationBuilder.DropColumn(
                name: "TokenHighestStreak",
                table: "VideoResults");

            migrationBuilder.DropColumn(
                name: "TokenQuestionReward",
                table: "VideoResults");

            migrationBuilder.DropColumn(
                name: "TokenSuperFire",
                table: "VideoResults");

            migrationBuilder.DropColumn(
                name: "TokenDone",
                table: "SectionGroupResults");

            migrationBuilder.DropColumn(
                name: "TokenHighestStreak",
                table: "SectionGroupResults");

            migrationBuilder.DropColumn(
                name: "TokenSuperFire",
                table: "SectionGroupResults");

            migrationBuilder.DropColumn(
                name: "TokenDone",
                table: "MockTestResults");

            migrationBuilder.DropColumn(
                name: "TokenHighestStreak",
                table: "MockTestResults");

            migrationBuilder.DropColumn(
                name: "TokenSuperFire",
                table: "MockTestResults");

            migrationBuilder.DropColumn(
                name: "TokenDone",
                table: "FinalTestResults");

            migrationBuilder.DropColumn(
                name: "TokenHighestStreak",
                table: "FinalTestResults");

            migrationBuilder.DropColumn(
                name: "TokenSuperFire",
                table: "FinalTestResults");
        }
    }
}
