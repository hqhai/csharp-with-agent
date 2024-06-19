using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_SectionGroupResult : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PlacementTestResultId",
                table: "SectionGroupResults",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SectionGroupResultId",
                table: "PlacementTestAnswers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SectionGroupResultId",
                table: "MockTestAnswers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SectionGroupResultId",
                table: "FinalTestAnswers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SectionGroupResultId",
                table: "ExtraPracticeAnswers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SectionGroupResults_PlacementTestResultId",
                table: "SectionGroupResults",
                column: "PlacementTestResultId");

            migrationBuilder.CreateIndex(
                name: "IX_PlacementTestAnswers_SectionGroupResultId",
                table: "PlacementTestAnswers",
                column: "SectionGroupResultId");

            migrationBuilder.CreateIndex(
                name: "IX_MockTestAnswers_SectionGroupResultId",
                table: "MockTestAnswers",
                column: "SectionGroupResultId");

            migrationBuilder.CreateIndex(
                name: "IX_FinalTestAnswers_SectionGroupResultId",
                table: "FinalTestAnswers",
                column: "SectionGroupResultId");

            migrationBuilder.CreateIndex(
                name: "IX_ExtraPracticeAnswers_SectionGroupResultId",
                table: "ExtraPracticeAnswers",
                column: "SectionGroupResultId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExtraPracticeAnswers_SectionGroupResults_SectionGroupResultId",
                table: "ExtraPracticeAnswers",
                column: "SectionGroupResultId",
                principalTable: "SectionGroupResults",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FinalTestAnswers_SectionGroupResults_SectionGroupResultId",
                table: "FinalTestAnswers",
                column: "SectionGroupResultId",
                principalTable: "SectionGroupResults",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MockTestAnswers_SectionGroupResults_SectionGroupResultId",
                table: "MockTestAnswers",
                column: "SectionGroupResultId",
                principalTable: "SectionGroupResults",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PlacementTestAnswers_SectionGroupResults_SectionGroupResultId",
                table: "PlacementTestAnswers",
                column: "SectionGroupResultId",
                principalTable: "SectionGroupResults",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SectionGroupResults_PlacementTestResults_PlacementTestResultId",
                table: "SectionGroupResults",
                column: "PlacementTestResultId",
                principalTable: "PlacementTestResults",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExtraPracticeAnswers_SectionGroupResults_SectionGroupResultId",
                table: "ExtraPracticeAnswers");

            migrationBuilder.DropForeignKey(
                name: "FK_FinalTestAnswers_SectionGroupResults_SectionGroupResultId",
                table: "FinalTestAnswers");

            migrationBuilder.DropForeignKey(
                name: "FK_MockTestAnswers_SectionGroupResults_SectionGroupResultId",
                table: "MockTestAnswers");

            migrationBuilder.DropForeignKey(
                name: "FK_PlacementTestAnswers_SectionGroupResults_SectionGroupResultId",
                table: "PlacementTestAnswers");

            migrationBuilder.DropForeignKey(
                name: "FK_SectionGroupResults_PlacementTestResults_PlacementTestResultId",
                table: "SectionGroupResults");

            migrationBuilder.DropIndex(
                name: "IX_SectionGroupResults_PlacementTestResultId",
                table: "SectionGroupResults");

            migrationBuilder.DropIndex(
                name: "IX_PlacementTestAnswers_SectionGroupResultId",
                table: "PlacementTestAnswers");

            migrationBuilder.DropIndex(
                name: "IX_MockTestAnswers_SectionGroupResultId",
                table: "MockTestAnswers");

            migrationBuilder.DropIndex(
                name: "IX_FinalTestAnswers_SectionGroupResultId",
                table: "FinalTestAnswers");

            migrationBuilder.DropIndex(
                name: "IX_ExtraPracticeAnswers_SectionGroupResultId",
                table: "ExtraPracticeAnswers");

            migrationBuilder.DropColumn(
                name: "PlacementTestResultId",
                table: "SectionGroupResults");

            migrationBuilder.DropColumn(
                name: "SectionGroupResultId",
                table: "PlacementTestAnswers");

            migrationBuilder.DropColumn(
                name: "SectionGroupResultId",
                table: "MockTestAnswers");

            migrationBuilder.DropColumn(
                name: "SectionGroupResultId",
                table: "FinalTestAnswers");

            migrationBuilder.DropColumn(
                name: "SectionGroupResultId",
                table: "ExtraPracticeAnswers");
        }
    }
}
