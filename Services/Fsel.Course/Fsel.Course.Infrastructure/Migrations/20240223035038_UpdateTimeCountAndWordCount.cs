using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTimeCountAndWordCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TimeCount",
                table: "MockTestAnswers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WordCount",
                table: "MockTestAnswers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WordCount",
                table: "ClassForumResults",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TimeCount",
                table: "ClassForumResultFiles",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TimeCount",
                table: "MockTestAnswers");

            migrationBuilder.DropColumn(
                name: "WordCount",
                table: "MockTestAnswers");

            migrationBuilder.DropColumn(
                name: "WordCount",
                table: "ClassForumResults");

            migrationBuilder.DropColumn(
                name: "TimeCount",
                table: "ClassForumResultFiles");
        }
    }
}
