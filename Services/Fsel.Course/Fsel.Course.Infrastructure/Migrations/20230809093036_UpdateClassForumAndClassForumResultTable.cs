using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateClassForumAndClassForumResultTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "ClassForumResults");

            migrationBuilder.AddColumn<bool>(
                name: "IsAlFeedBack",
                table: "ClassForums",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<double>(
                name: "SettingFrequecy",
                table: "ClassForums",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "SettingModel",
                table: "ClassForums",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "SettingPresence",
                table: "ClassForums",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "SettingTemperature",
                table: "ClassForums",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "SettingTopP",
                table: "ClassForums",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "SettingWordMaxLength",
                table: "ClassForums",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "SystemRoleAlConfig",
                table: "ClassForums",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserAlConfig",
                table: "ClassForums",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GradingAlFeedback",
                table: "ClassForumResults",
                type: "nvarchar(max)",
                maxLength: 10000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WordContent",
                table: "ClassForumResults",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAlFeedBack",
                table: "ClassForums");

            migrationBuilder.DropColumn(
                name: "SettingFrequecy",
                table: "ClassForums");

            migrationBuilder.DropColumn(
                name: "SettingModel",
                table: "ClassForums");

            migrationBuilder.DropColumn(
                name: "SettingPresence",
                table: "ClassForums");

            migrationBuilder.DropColumn(
                name: "SettingTemperature",
                table: "ClassForums");

            migrationBuilder.DropColumn(
                name: "SettingTopP",
                table: "ClassForums");

            migrationBuilder.DropColumn(
                name: "SettingWordMaxLength",
                table: "ClassForums");

            migrationBuilder.DropColumn(
                name: "SystemRoleAlConfig",
                table: "ClassForums");

            migrationBuilder.DropColumn(
                name: "UserAlConfig",
                table: "ClassForums");

            migrationBuilder.DropColumn(
                name: "GradingAlFeedback",
                table: "ClassForumResults");

            migrationBuilder.DropColumn(
                name: "WordContent",
                table: "ClassForumResults");

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "ClassForumResults",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);
        }
    }
}
