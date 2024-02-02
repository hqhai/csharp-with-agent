using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_Result_TokenCoin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TokenDone",
                table: "VideoTimeCodeResults");

            migrationBuilder.DropColumn(
                name: "TokenHighestStreak",
                table: "VideoTimeCodeResults");

            migrationBuilder.DropColumn(
                name: "TokenDone",
                table: "VideoResults");

            migrationBuilder.DropColumn(
                name: "TokenHighestStreak",
                table: "VideoResults");

            migrationBuilder.DropColumn(
                name: "TokenDone",
                table: "MockTestResults");

            migrationBuilder.DropColumn(
                name: "TokenHighestStreak",
                table: "MockTestResults");

            migrationBuilder.DropColumn(
                name: "TokenDone",
                table: "FinalTestResults");

            migrationBuilder.DropColumn(
                name: "TokenHighestStreak",
                table: "FinalTestResults");

            migrationBuilder.RenameColumn(
                name: "TokenSuperFire",
                table: "VideoTimeCodeResults",
                newName: "TokenLastTime");

            migrationBuilder.RenameColumn(
                name: "TokenQuestionReward",
                table: "VideoTimeCodeResults",
                newName: "TokenFirstTime");

            migrationBuilder.RenameColumn(
                name: "TokenSuperFire",
                table: "VideoResults",
                newName: "TokenLastTime");

            migrationBuilder.RenameColumn(
                name: "TokenQuestionReward",
                table: "VideoResults",
                newName: "TokenFirstTime");

            migrationBuilder.RenameColumn(
                name: "TokenSuperFire",
                table: "MockTestResults",
                newName: "TokenLastTime");

            migrationBuilder.RenameColumn(
                name: "TokenQuestionReward",
                table: "MockTestResults",
                newName: "TokenFirstTime");

            migrationBuilder.RenameColumn(
                name: "TokenSuperFire",
                table: "FinalTestResults",
                newName: "TokenLastTime");

            migrationBuilder.RenameColumn(
                name: "TokenQuestionReward",
                table: "FinalTestResults",
                newName: "TokenFirstTime");

            migrationBuilder.AddColumn<int>(
                name: "TokenFirstTime",
                table: "HomeWorkResults",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TokenLastTime",
                table: "HomeWorkResults",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TokenFirstTime",
                table: "ClassForumResults",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TokenLastTime",
                table: "ClassForumResults",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TokenFirstTime",
                table: "HomeWorkResults");

            migrationBuilder.DropColumn(
                name: "TokenLastTime",
                table: "HomeWorkResults");

            migrationBuilder.DropColumn(
                name: "TokenFirstTime",
                table: "ClassForumResults");

            migrationBuilder.DropColumn(
                name: "TokenLastTime",
                table: "ClassForumResults");

            migrationBuilder.RenameColumn(
                name: "TokenLastTime",
                table: "VideoTimeCodeResults",
                newName: "TokenSuperFire");

            migrationBuilder.RenameColumn(
                name: "TokenFirstTime",
                table: "VideoTimeCodeResults",
                newName: "TokenQuestionReward");

            migrationBuilder.RenameColumn(
                name: "TokenLastTime",
                table: "VideoResults",
                newName: "TokenSuperFire");

            migrationBuilder.RenameColumn(
                name: "TokenFirstTime",
                table: "VideoResults",
                newName: "TokenQuestionReward");

            migrationBuilder.RenameColumn(
                name: "TokenLastTime",
                table: "MockTestResults",
                newName: "TokenSuperFire");

            migrationBuilder.RenameColumn(
                name: "TokenFirstTime",
                table: "MockTestResults",
                newName: "TokenQuestionReward");

            migrationBuilder.RenameColumn(
                name: "TokenLastTime",
                table: "FinalTestResults",
                newName: "TokenSuperFire");

            migrationBuilder.RenameColumn(
                name: "TokenFirstTime",
                table: "FinalTestResults",
                newName: "TokenQuestionReward");

            migrationBuilder.AddColumn<int>(
                name: "TokenDone",
                table: "VideoTimeCodeResults",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TokenHighestStreak",
                table: "VideoTimeCodeResults",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TokenDone",
                table: "VideoResults",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TokenHighestStreak",
                table: "VideoResults",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TokenDone",
                table: "MockTestResults",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TokenHighestStreak",
                table: "MockTestResults",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TokenDone",
                table: "FinalTestResults",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TokenHighestStreak",
                table: "FinalTestResults",
                type: "int",
                nullable: true);
        }
    }
}
