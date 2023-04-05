using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Updatequestion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VideoTimeCodeAnswers_Questions_QuestionId",
                table: "VideoTimeCodeAnswers");

            migrationBuilder.AddForeignKey(
                name: "FK_VideoTimeCodeAnswers_Questions_QuestionId",
                table: "VideoTimeCodeAnswers",
                column: "QuestionId",
                principalTable: "Questions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VideoTimeCodeAnswers_Questions_QuestionId",
                table: "VideoTimeCodeAnswers");

            migrationBuilder.AddForeignKey(
                name: "FK_VideoTimeCodeAnswers_Questions_QuestionId",
                table: "VideoTimeCodeAnswers",
                column: "QuestionId",
                principalTable: "Questions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
