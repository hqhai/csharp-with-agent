using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_ExtraPracticeAnswerTimeCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "VideoTimeCodeId",
                table: "ExtraPracticeAnswers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExtraPracticeAnswers_VideoTimeCodeId",
                table: "ExtraPracticeAnswers",
                column: "VideoTimeCodeId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExtraPracticeAnswers_VideoTimeCodes_VideoTimeCodeId",
                table: "ExtraPracticeAnswers",
                column: "VideoTimeCodeId",
                principalTable: "VideoTimeCodes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExtraPracticeAnswers_VideoTimeCodes_VideoTimeCodeId",
                table: "ExtraPracticeAnswers");

            migrationBuilder.DropIndex(
                name: "IX_ExtraPracticeAnswers_VideoTimeCodeId",
                table: "ExtraPracticeAnswers");

            migrationBuilder.DropColumn(
                name: "VideoTimeCodeId",
                table: "ExtraPracticeAnswers");
        }
    }
}
