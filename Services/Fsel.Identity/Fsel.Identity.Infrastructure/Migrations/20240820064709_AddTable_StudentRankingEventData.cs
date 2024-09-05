using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Identity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTable_StudentRankingEventData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudentRankingEvents_CompetitionEvents_CompetitionEventId",
                table: "StudentRankingEvents");

            migrationBuilder.RenameColumn(
                name: "SchoolCode",
                table: "StudentCompetitionSnapShots",
                newName: "EventCode");

            migrationBuilder.AddColumn<string>(
                name: "SchoolClass",
                table: "Students",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SchoolGrade",
                table: "Students",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "CompetitionEventId",
                table: "StudentRankingEvents",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<Guid>(
                name: "CourseResultId",
                table: "StudentRankingEvents",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "Grade",
                table: "StudentRankingEvents",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "OverallScore",
                table: "StudentRankingEvents",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Proccess",
                table: "StudentRankingEvents",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "RankingScore",
                table: "StudentRankingEvents",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "StudentRankingEvents",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "StudentCompetitionEvents",
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
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompetitionEventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentCompetitionEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentCompetitionEvents_CompetitionEvents_CompetitionEventId",
                        column: x => x.CompetitionEventId,
                        principalTable: "CompetitionEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StudentCompetitionEvents_CompetitionEventId",
                table: "StudentCompetitionEvents",
                column: "CompetitionEventId");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentRankingEvents_CompetitionEvents_CompetitionEventId",
                table: "StudentRankingEvents",
                column: "CompetitionEventId",
                principalTable: "CompetitionEvents",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudentRankingEvents_CompetitionEvents_CompetitionEventId",
                table: "StudentRankingEvents");

            migrationBuilder.DropTable(
                name: "StudentCompetitionEvents");

            migrationBuilder.DropColumn(
                name: "SchoolClass",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "SchoolGrade",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "CourseResultId",
                table: "StudentRankingEvents");

            migrationBuilder.DropColumn(
                name: "Grade",
                table: "StudentRankingEvents");

            migrationBuilder.DropColumn(
                name: "OverallScore",
                table: "StudentRankingEvents");

            migrationBuilder.DropColumn(
                name: "Proccess",
                table: "StudentRankingEvents");

            migrationBuilder.DropColumn(
                name: "RankingScore",
                table: "StudentRankingEvents");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "StudentRankingEvents");

            migrationBuilder.RenameColumn(
                name: "EventCode",
                table: "StudentCompetitionSnapShots",
                newName: "SchoolCode");

            migrationBuilder.AlterColumn<Guid>(
                name: "CompetitionEventId",
                table: "StudentRankingEvents",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentRankingEvents_CompetitionEvents_CompetitionEventId",
                table: "StudentRankingEvents",
                column: "CompetitionEventId",
                principalTable: "CompetitionEvents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
