using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCoLumnCriteria_TableMockTestAnswer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Conherence",
                table: "MockTestAnswers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GrammaticalRage",
                table: "MockTestAnswers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LexicalResourse",
                table: "MockTestAnswers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TaskResponse",
                table: "MockTestAnswers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Conherence",
                table: "MockTestAnswers");

            migrationBuilder.DropColumn(
                name: "GrammaticalRage",
                table: "MockTestAnswers");

            migrationBuilder.DropColumn(
                name: "LexicalResourse",
                table: "MockTestAnswers");

            migrationBuilder.DropColumn(
                name: "TaskResponse",
                table: "MockTestAnswers");
        }
    }
}
