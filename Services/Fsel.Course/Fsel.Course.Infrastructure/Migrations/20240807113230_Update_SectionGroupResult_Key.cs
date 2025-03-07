using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_SectionGroupResult_Key : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SectionGroupResults_SectionGroupId_MockTestResultId_FinalTestResultId_PlacementTestResultId",
                table: "SectionGroupResults");

            migrationBuilder.DropIndex(
                name: "IX_MockTestAnswers_MockTestResultId_SectionQuestionId_SectionGroupResultId_SectionTimeCodeId_SectionId",
                table: "MockTestAnswers");

            migrationBuilder.CreateIndex(
                name: "IX_SectionGroupResults_SectionGroupId_FinalTestResultId",
                table: "SectionGroupResults",
                columns: new[] { "SectionGroupId", "FinalTestResultId" },
                unique: true,
                filter: "[FinalTestResultId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SectionGroupResults_SectionGroupId_MockTestResultId",
                table: "SectionGroupResults",
                columns: new[] { "SectionGroupId", "MockTestResultId" },
                unique: true,
                filter: "[MockTestResultId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SectionGroupResults_SectionGroupId_PlacementTestResultId",
                table: "SectionGroupResults",
                columns: new[] { "SectionGroupId", "PlacementTestResultId" },
                unique: true,
                filter: "[PlacementTestResultId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MockTestAnswers_MockTestResultId_SectionGroupResultId_SectionId",
                table: "MockTestAnswers",
                columns: new[] { "MockTestResultId", "SectionGroupResultId", "SectionId" },
                unique: true,
                filter: "[SectionGroupResultId] IS NOT NULL AND [SectionId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MockTestAnswers_MockTestResultId_SectionGroupResultId_SectionQuestionId",
                table: "MockTestAnswers",
                columns: new[] { "MockTestResultId", "SectionGroupResultId", "SectionQuestionId" },
                unique: true,
                filter: "[SectionGroupResultId] IS NOT NULL AND [SectionQuestionId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MockTestAnswers_MockTestResultId_SectionGroupResultId_SectionTimeCodeId",
                table: "MockTestAnswers",
                columns: new[] { "MockTestResultId", "SectionGroupResultId", "SectionTimeCodeId" },
                unique: true,
                filter: "[SectionGroupResultId] IS NOT NULL AND [SectionTimeCodeId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SectionGroupResults_SectionGroupId_FinalTestResultId",
                table: "SectionGroupResults");

            migrationBuilder.DropIndex(
                name: "IX_SectionGroupResults_SectionGroupId_MockTestResultId",
                table: "SectionGroupResults");

            migrationBuilder.DropIndex(
                name: "IX_SectionGroupResults_SectionGroupId_PlacementTestResultId",
                table: "SectionGroupResults");

            migrationBuilder.DropIndex(
                name: "IX_MockTestAnswers_MockTestResultId_SectionGroupResultId_SectionId",
                table: "MockTestAnswers");

            migrationBuilder.DropIndex(
                name: "IX_MockTestAnswers_MockTestResultId_SectionGroupResultId_SectionQuestionId",
                table: "MockTestAnswers");

            migrationBuilder.DropIndex(
                name: "IX_MockTestAnswers_MockTestResultId_SectionGroupResultId_SectionTimeCodeId",
                table: "MockTestAnswers");

            migrationBuilder.CreateIndex(
                name: "IX_SectionGroupResults_SectionGroupId_MockTestResultId_FinalTestResultId_PlacementTestResultId",
                table: "SectionGroupResults",
                columns: new[] { "SectionGroupId", "MockTestResultId", "FinalTestResultId", "PlacementTestResultId" },
                unique: true,
                filter: "[MockTestResultId] IS NOT NULL AND [FinalTestResultId] IS NOT NULL AND [PlacementTestResultId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MockTestAnswers_MockTestResultId_SectionQuestionId_SectionGroupResultId_SectionTimeCodeId_SectionId",
                table: "MockTestAnswers",
                columns: new[] { "MockTestResultId", "SectionQuestionId", "SectionGroupResultId", "SectionTimeCodeId", "SectionId" },
                unique: true,
                filter: "[SectionQuestionId] IS NOT NULL AND [SectionGroupResultId] IS NOT NULL AND [SectionTimeCodeId] IS NOT NULL AND [SectionId] IS NOT NULL");
        }
    }
}
