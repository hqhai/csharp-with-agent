using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateTimeCodeTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Excercises",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedUserName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UpdatedUserName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DeletedUserName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    QuestionType = table.Column<int>(type: "int", nullable: false),
                    MediaPost = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    CourseSkill = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Excercises", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Questions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedUserName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UpdatedUserName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DeletedUserName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    EnumQuestionType = table.Column<int>(type: "int", nullable: false),
                    IsSave = table.Column<bool>(type: "bit", nullable: false),
                    Config = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Questions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VideoTimeCodes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedUserName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UpdatedUserName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DeletedUserName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DisplayTimeTicks = table.Column<long>(type: "bigint", nullable: false),
                    ExecutionTimeTicks = table.Column<long>(type: "bigint", nullable: false),
                    TimeCodeType = table.Column<int>(type: "int", nullable: false),
                    VideoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VideoTimeCodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VideoTimeCodes_Videos_VideoId",
                        column: x => x.VideoId,
                        principalTable: "Videos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExcerciseQuestions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedUserName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UpdatedUserName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DeletedUserName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    ExcerciseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExcerciseQuestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExcerciseQuestions_Excercises_ExcerciseId",
                        column: x => x.ExcerciseId,
                        principalTable: "Excercises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExcerciseQuestions_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TimeCodeExcercises",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedUserName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UpdatedUserName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DeletedUserName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    ExcerciseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VideoTimeCodeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeCodeExcercises", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TimeCodeExcercises_Excercises_ExcerciseId",
                        column: x => x.ExcerciseId,
                        principalTable: "Excercises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TimeCodeExcercises_VideoTimeCodes_VideoTimeCodeId",
                        column: x => x.VideoTimeCodeId,
                        principalTable: "VideoTimeCodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExcerciseQuestions_ExcerciseId",
                table: "ExcerciseQuestions",
                column: "ExcerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_ExcerciseQuestions_QuestionId",
                table: "ExcerciseQuestions",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_TimeCodeExcercises_ExcerciseId",
                table: "TimeCodeExcercises",
                column: "ExcerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_TimeCodeExcercises_VideoTimeCodeId",
                table: "TimeCodeExcercises",
                column: "VideoTimeCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_VideoTimeCodes_VideoId",
                table: "VideoTimeCodes",
                column: "VideoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExcerciseQuestions");

            migrationBuilder.DropTable(
                name: "TimeCodeExcercises");

            migrationBuilder.DropTable(
                name: "Questions");

            migrationBuilder.DropTable(
                name: "Excercises");

            migrationBuilder.DropTable(
                name: "VideoTimeCodes");
        }
    }
}
