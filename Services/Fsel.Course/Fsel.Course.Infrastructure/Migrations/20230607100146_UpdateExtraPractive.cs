using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateExtraPractive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FilePath",
                table: "ExtraPractices",
                newName: "Abstract");

            migrationBuilder.AddColumn<string>(
                name: "Author",
                table: "ExtraPractices",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "ExtraPractices",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FilePathsStr",
                table: "ExtraPractices",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "VideoId",
                table: "ExtraPractices",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VideoLink",
                table: "ExtraPractices",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ExtraPracticeChapters",
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
                    Name = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    PageNumber = table.Column<int>(type: "int", nullable: false),
                    ExtraPracticeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExtraPracticeChapters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExtraPracticeChapters_ExtraPractices_ExtraPracticeId",
                        column: x => x.ExtraPracticeId,
                        principalTable: "ExtraPractices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExtraPracticeExercises",
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
                    ExtraPracticeChapterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ExtraPracticeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExerciseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExtraPracticeExercises", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExtraPracticeExercises_Exercises_ExerciseId",
                        column: x => x.ExerciseId,
                        principalTable: "Exercises",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ExtraPracticeExercises_ExtraPracticeChapters_ExtraPracticeChapterId",
                        column: x => x.ExtraPracticeChapterId,
                        principalTable: "ExtraPracticeChapters",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ExtraPracticeExercises_ExtraPractices_ExtraPracticeId",
                        column: x => x.ExtraPracticeId,
                        principalTable: "ExtraPractices",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExtraPractices_VideoId",
                table: "ExtraPractices",
                column: "VideoId");

            migrationBuilder.CreateIndex(
                name: "IX_ExtraPracticeChapters_ExtraPracticeId",
                table: "ExtraPracticeChapters",
                column: "ExtraPracticeId");

            migrationBuilder.CreateIndex(
                name: "IX_ExtraPracticeExercises_ExerciseId",
                table: "ExtraPracticeExercises",
                column: "ExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_ExtraPracticeExercises_ExtraPracticeChapterId",
                table: "ExtraPracticeExercises",
                column: "ExtraPracticeChapterId");

            migrationBuilder.CreateIndex(
                name: "IX_ExtraPracticeExercises_ExtraPracticeId",
                table: "ExtraPracticeExercises",
                column: "ExtraPracticeId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExtraPractices_Videos_VideoId",
                table: "ExtraPractices",
                column: "VideoId",
                principalTable: "Videos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExtraPractices_Videos_VideoId",
                table: "ExtraPractices");

            migrationBuilder.DropTable(
                name: "ExtraPracticeExercises");

            migrationBuilder.DropTable(
                name: "ExtraPracticeChapters");

            migrationBuilder.DropIndex(
                name: "IX_ExtraPractices_VideoId",
                table: "ExtraPractices");

            migrationBuilder.DropColumn(
                name: "Author",
                table: "ExtraPractices");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "ExtraPractices");

            migrationBuilder.DropColumn(
                name: "FilePathsStr",
                table: "ExtraPractices");

            migrationBuilder.DropColumn(
                name: "VideoId",
                table: "ExtraPractices");

            migrationBuilder.DropColumn(
                name: "VideoLink",
                table: "ExtraPractices");

            migrationBuilder.RenameColumn(
                name: "Abstract",
                table: "ExtraPractices",
                newName: "FilePath");
        }
    }
}
