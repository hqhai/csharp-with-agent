using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMockTestResult : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FeedbackStars",
                table: "MockTestResults",
                newName: "FeedBackStars");

            migrationBuilder.RenameColumn(
                name: "FeedbackNote",
                table: "MockTestResults",
                newName: "FeedBackNote");

            migrationBuilder.AlterColumn<Guid>(
                name: "TeacherId",
                table: "Videos",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "FeedBackStars",
                table: "MockTestResults",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<Guid>(
                name: "GradingTeacherId",
                table: "MockTestResults",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GradingTeacherId",
                table: "MockTestResults");

            migrationBuilder.RenameColumn(
                name: "FeedBackStars",
                table: "MockTestResults",
                newName: "FeedbackStars");

            migrationBuilder.RenameColumn(
                name: "FeedBackNote",
                table: "MockTestResults",
                newName: "FeedbackNote");

            migrationBuilder.AlterColumn<Guid>(
                name: "TeacherId",
                table: "Videos",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<int>(
                name: "FeedbackStars",
                table: "MockTestResults",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
