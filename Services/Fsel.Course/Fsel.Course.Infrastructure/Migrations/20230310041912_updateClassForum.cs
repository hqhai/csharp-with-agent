using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class updateClassForum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LessonVideo");

            migrationBuilder.DropIndex(
                name: "IX_ClassForums_LessonId",
                table: "ClassForums");

            migrationBuilder.DropColumn(
                name: "TaggetTimeLimit",
                table: "ClassForums");

            migrationBuilder.AlterColumn<Guid>(
                name: "LessonId",
                table: "LessonHomeWorks",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "HomeWorkId",
                table: "LessonHomeWorks",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "LessonId",
                table: "LessonExtraPractices",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "ExtracPraticeId",
                table: "LessonExtraPractices",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "TaggetWordLimit",
                table: "ClassForums",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(TimeSpan),
                oldType: "time",
                oldNullable: true);

            migrationBuilder.AddColumn<long>(
                name: "TaggetTimeLimitTicks",
                table: "ClassForums",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_ClassForums_LessonId",
                table: "ClassForums",
                column: "LessonId",
                unique: true,
                filter: "[LessonId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_ClassForums_Lessons_LessonId",
                table: "ClassForums",
                column: "LessonId",
                principalTable: "Lessons",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LessonExtraPractices_ExtraPractices_ExtracPraticeId",
                table: "LessonExtraPractices",
                column: "ExtracPraticeId",
                principalTable: "ExtraPractices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LessonExtraPractices_Lessons_LessonId",
                table: "LessonExtraPractices",
                column: "LessonId",
                principalTable: "Lessons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LessonHomeWorks_HomeWorks_HomeWorkId",
                table: "LessonHomeWorks",
                column: "HomeWorkId",
                principalTable: "HomeWorks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LessonHomeWorks_Lessons_LessonId",
                table: "LessonHomeWorks",
                column: "LessonId",
                principalTable: "Lessons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LessonVideos_Lessons_LessonId",
                table: "LessonVideos",
                column: "LessonId",
                principalTable: "Lessons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LessonVideos_Videos_VideoId",
                table: "LessonVideos",
                column: "VideoId",
                principalTable: "Videos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClassForums_Lessons_LessonId",
                table: "ClassForums");

            migrationBuilder.DropForeignKey(
                name: "FK_LessonExtraPractices_ExtraPractices_ExtracPraticeId",
                table: "LessonExtraPractices");

            migrationBuilder.DropForeignKey(
                name: "FK_LessonExtraPractices_Lessons_LessonId",
                table: "LessonExtraPractices");

            migrationBuilder.DropForeignKey(
                name: "FK_LessonHomeWorks_HomeWorks_HomeWorkId",
                table: "LessonHomeWorks");

            migrationBuilder.DropForeignKey(
                name: "FK_LessonHomeWorks_Lessons_LessonId",
                table: "LessonHomeWorks");

            migrationBuilder.DropForeignKey(
                name: "FK_LessonVideos_Lessons_LessonId",
                table: "LessonVideos");

            migrationBuilder.DropForeignKey(
                name: "FK_LessonVideos_Videos_VideoId",
                table: "LessonVideos");

            migrationBuilder.DropIndex(
                name: "IX_ClassForums_LessonId",
                table: "ClassForums");

            migrationBuilder.DropColumn(
                name: "TaggetTimeLimitTicks",
                table: "ClassForums");

            migrationBuilder.AlterColumn<Guid>(
                name: "LessonId",
                table: "LessonHomeWorks",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "HomeWorkId",
                table: "LessonHomeWorks",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "LessonId",
                table: "LessonExtraPractices",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "ExtracPraticeId",
                table: "LessonExtraPractices",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<TimeSpan>(
                name: "TaggetWordLimit",
                table: "ClassForums",
                type: "time",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "TaggetTimeLimit",
                table: "ClassForums",
                type: "time",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LessonVideo",
                columns: table => new
                {
                    LessonId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    VideoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.ForeignKey(
                        name: "FK_LessonVideo_Lessons_LessonId",
                        column: x => x.LessonId,
                        principalTable: "Lessons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LessonVideo_Videos_VideoId",
                        column: x => x.VideoId,
                        principalTable: "Videos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClassForums_LessonId",
                table: "ClassForums",
                column: "LessonId");
        }
    }
}
