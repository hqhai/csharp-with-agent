using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Create_VideoTimeCodeResult : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "VideoTimeCodeId",
                table: "VideoTimeCodeAnswers",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "VideoResultId",
                table: "VideoTimeCodeAnswers",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<Guid>(
                name: "VideoTimeCodeResultId",
                table: "VideoTimeCodeAnswers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "VideoTimeCodeResults",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedFullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UpdatedFullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DeletedFullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    VideoResultId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VideoTimeCodeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CorrectCount = table.Column<int>(type: "int", nullable: false),
                    CorrectTotal = table.Column<int>(type: "int", nullable: false),
                    Percent = table.Column<double>(type: "float", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SkillScoresStr = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VideoTimeCodeResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VideoTimeCodeResults_VideoResults_VideoResultId",
                        column: x => x.VideoResultId,
                        principalTable: "VideoResults",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_VideoTimeCodeResults_VideoTimeCodes_VideoTimeCodeId",
                        column: x => x.VideoTimeCodeId,
                        principalTable: "VideoTimeCodes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_VideoTimeCodeAnswers_VideoTimeCodeResultId",
                table: "VideoTimeCodeAnswers",
                column: "VideoTimeCodeResultId");

            migrationBuilder.CreateIndex(
                name: "IX_VideoTimeCodeResults_VideoResultId",
                table: "VideoTimeCodeResults",
                column: "VideoResultId");

            migrationBuilder.CreateIndex(
                name: "IX_VideoTimeCodeResults_VideoTimeCodeId",
                table: "VideoTimeCodeResults",
                column: "VideoTimeCodeId");

            migrationBuilder.AddForeignKey(
                name: "FK_VideoTimeCodeAnswers_VideoTimeCodeResults_VideoTimeCodeResultId",
                table: "VideoTimeCodeAnswers",
                column: "VideoTimeCodeResultId",
                principalTable: "VideoTimeCodeResults",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VideoTimeCodeAnswers_VideoTimeCodeResults_VideoTimeCodeResultId",
                table: "VideoTimeCodeAnswers");

            migrationBuilder.DropTable(
                name: "VideoTimeCodeResults");

            migrationBuilder.DropIndex(
                name: "IX_VideoTimeCodeAnswers_VideoTimeCodeResultId",
                table: "VideoTimeCodeAnswers");

            migrationBuilder.DropColumn(
                name: "VideoTimeCodeResultId",
                table: "VideoTimeCodeAnswers");

            migrationBuilder.AlterColumn<Guid>(
                name: "VideoTimeCodeId",
                table: "VideoTimeCodeAnswers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "VideoResultId",
                table: "VideoTimeCodeAnswers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);
        }
    }
}
