using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_QuestionExplanationError : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuestionExplanationErrors_VideoResults_VideoResultId",
                table: "QuestionExplanationErrors");

            migrationBuilder.DropIndex(
                name: "IX_QuestionExplanationErrors_VideoResultId",
                table: "QuestionExplanationErrors");

            migrationBuilder.RenameColumn(
                name: "VideoResultId",
                table: "QuestionExplanationErrors",
                newName: "ObjectResultId");

            migrationBuilder.AddColumn<string>(
                name: "ExplanationType",
                table: "QuestionExplanationErrors",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExplanationType",
                table: "QuestionExplanationErrors");

            migrationBuilder.RenameColumn(
                name: "ObjectResultId",
                table: "QuestionExplanationErrors",
                newName: "VideoResultId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionExplanationErrors_VideoResultId",
                table: "QuestionExplanationErrors",
                column: "VideoResultId");

            migrationBuilder.AddForeignKey(
                name: "FK_QuestionExplanationErrors_VideoResults_VideoResultId",
                table: "QuestionExplanationErrors",
                column: "VideoResultId",
                principalTable: "VideoResults",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
