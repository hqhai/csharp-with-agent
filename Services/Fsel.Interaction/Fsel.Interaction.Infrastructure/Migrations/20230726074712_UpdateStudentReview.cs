using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Interaction.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateStudentReview : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudentReviewDetail_StudentReviews_StudentReviewId",
                table: "StudentReviewDetail");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StudentReviewDetail",
                table: "StudentReviewDetail");

            migrationBuilder.RenameTable(
                name: "StudentReviewDetail",
                newName: "StudentReviewDetails");

            migrationBuilder.RenameIndex(
                name: "IX_StudentReviewDetail_StudentReviewId",
                table: "StudentReviewDetails",
                newName: "IX_StudentReviewDetails_StudentReviewId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StudentReviewDetails",
                table: "StudentReviewDetails",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentReviewDetails_StudentReviews_StudentReviewId",
                table: "StudentReviewDetails",
                column: "StudentReviewId",
                principalTable: "StudentReviews",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudentReviewDetails_StudentReviews_StudentReviewId",
                table: "StudentReviewDetails");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StudentReviewDetails",
                table: "StudentReviewDetails");

            migrationBuilder.RenameTable(
                name: "StudentReviewDetails",
                newName: "StudentReviewDetail");

            migrationBuilder.RenameIndex(
                name: "IX_StudentReviewDetails_StudentReviewId",
                table: "StudentReviewDetail",
                newName: "IX_StudentReviewDetail_StudentReviewId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StudentReviewDetail",
                table: "StudentReviewDetail",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentReviewDetail_StudentReviews_StudentReviewId",
                table: "StudentReviewDetail",
                column: "StudentReviewId",
                principalTable: "StudentReviews",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
