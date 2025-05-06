using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.System.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Create_DailyQuizTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DailyQuizQuestions",
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
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SubCategory = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Explanation = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    TranslationsStr = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyQuizQuestions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DailyQuizWinners",
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
                    Code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SchoolId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompetitionEventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsWin = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyQuizWinners", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DailyQuizAnswers",
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
                    Content = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsCorrect = table.Column<bool>(type: "bit", nullable: false),
                    DailyQuizQuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TranslationsStr = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyQuizAnswers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DailyQuizAnswers_DailyQuizQuestions_DailyQuizQuestionId",
                        column: x => x.DailyQuizQuestionId,
                        principalTable: "DailyQuizQuestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DailyQuizHistories",
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
                    DailyQuizQuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DailyQuizAnswerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyQuizHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DailyQuizHistories_DailyQuizAnswers_DailyQuizAnswerId",
                        column: x => x.DailyQuizAnswerId,
                        principalTable: "DailyQuizAnswers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DailyQuizHistories_DailyQuizQuestions_DailyQuizQuestionId",
                        column: x => x.DailyQuizQuestionId,
                        principalTable: "DailyQuizQuestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "TokenConfigs",
                keyColumn: "Id",
                keyValue: new Guid("bc261be5-f658-415a-9498-8b813742708c"),
                column: "ConfigStr",
                value: "[{\"baseValue\":5,\"focustimeid\":\"0A7B57F3-C964-4F1B-8986-DF1579C5D08B\",\"totalActions\":720,\"description\":\"User is active in focus mode 15' continues\",\"displayOrder\":1},{\"baseValue\":15,\"focustimeid\":\"9B4FA7B6-1AF4-458B-82D9-621C1A88654A\",\"description\":\"User is active in focus mode 30' continues\",\"totalActions\":720,\"displayOrder\":2},{\"baseValue\":25,\"focustimeid\":\"124F4341-4C87-4E5F-BA4D-2481D8D36737\",\"description\":\"User is active in focus mode 45' continues\",\"totalActions\":720,\"displayOrder\":3},{\"baseValue\":40,\"focustimeid\":\"82061293-C9D0-4598-99F1-8DFD8162B999\",\"description\":\"User is active in focus mode 60' continues\",\"totalActions\":720,\"displayOrder\":4},{\"baseValue\":60,\"focustimeid\":\"FD7E66D3-A29B-4DA7-BB29-2283536D836A\",\"description\":\"User is active in focus mode 90' continues\",\"totalActions\":720,\"displayOrder\":5}]");

            migrationBuilder.UpdateData(
                table: "TokenConfigs",
                keyColumn: "Id",
                keyValue: new Guid("f08945d0-3b03-4299-ae22-e3ce2408b74b"),
                column: "ConfigStr",
                value: "[{\"baseValue\":5,\"focustimeid\":\"0A7B57F3-C964-4F1B-8986-DF1579C5D08B\",\"totalActions\":720,\"description\":\"User is active in focus mode 15' continues\",\"displayOrder\":1},{\"baseValue\":15,\"focustimeid\":\"9B4FA7B6-1AF4-458B-82D9-621C1A88654A\",\"description\":\"User is active in focus mode 30' continues\",\"totalActions\":720,\"displayOrder\":2},{\"baseValue\":25,\"focustimeid\":\"124F4341-4C87-4E5F-BA4D-2481D8D36737\",\"description\":\"User is active in focus mode 45' continues\",\"totalActions\":720,\"displayOrder\":3},{\"baseValue\":40,\"focustimeid\":\"82061293-C9D0-4598-99F1-8DFD8162B999\",\"description\":\"User is active in focus mode 60' continues\",\"totalActions\":720,\"displayOrder\":4},{\"baseValue\":60,\"focustimeid\":\"FD7E66D3-A29B-4DA7-BB29-2283536D836A\",\"description\":\"User is active in focus mode 90' continues\",\"totalActions\":720,\"displayOrder\":5}]");

            migrationBuilder.CreateIndex(
                name: "IX_DailyQuizAnswers_DailyQuizQuestionId",
                table: "DailyQuizAnswers",
                column: "DailyQuizQuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyQuizHistories_DailyQuizAnswerId",
                table: "DailyQuizHistories",
                column: "DailyQuizAnswerId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyQuizHistories_DailyQuizQuestionId",
                table: "DailyQuizHistories",
                column: "DailyQuizQuestionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DailyQuizHistories");

            migrationBuilder.DropTable(
                name: "DailyQuizWinners");

            migrationBuilder.DropTable(
                name: "DailyQuizAnswers");

            migrationBuilder.DropTable(
                name: "DailyQuizQuestions");

            migrationBuilder.UpdateData(
                table: "TokenConfigs",
                keyColumn: "Id",
                keyValue: new Guid("bc261be5-f658-415a-9498-8b813742708c"),
                column: "ConfigStr",
                value: "[{\"baseValue\":5,\"focustimeid\":\"0A7B57F3-C964-4F1B-8986-DF1579C5D08B\",\"totalActions\":720,\"description\":\"User is active in focus mode 15\\u0027 continues\",\"displayOrder\":1},{\"baseValue\":15,\"focustimeid\":\"9B4FA7B6-1AF4-458B-82D9-621C1A88654A\",\"description\":\"User is active in focus mode 30\\u0027 continues\",\"totalActions\":720,\"displayOrder\":2},{\"baseValue\":25,\"focustimeid\":\"124F4341-4C87-4E5F-BA4D-2481D8D36737\",\"description\":\"User is active in focus mode 45\\u0027 continues\",\"totalActions\":720,\"displayOrder\":3},{\"baseValue\":40,\"focustimeid\":\"82061293-C9D0-4598-99F1-8DFD8162B999\",\"description\":\"User is active in focus mode 60\\u0027 continues\",\"totalActions\":720,\"displayOrder\":4},{\"baseValue\":60,\"focustimeid\":\"FD7E66D3-A29B-4DA7-BB29-2283536D836A\",\"description\":\"User is active in focus mode 90\\u0027 continues\",\"totalActions\":720,\"displayOrder\":5}]");

            migrationBuilder.UpdateData(
                table: "TokenConfigs",
                keyColumn: "Id",
                keyValue: new Guid("f08945d0-3b03-4299-ae22-e3ce2408b74b"),
                column: "ConfigStr",
                value: "[{\"baseValue\":5,\"focustimeid\":\"0A7B57F3-C964-4F1B-8986-DF1579C5D08B\",\"totalActions\":720,\"description\":\"User is active in focus mode 15\\u0027 continues\",\"displayOrder\":1},{\"baseValue\":15,\"focustimeid\":\"9B4FA7B6-1AF4-458B-82D9-621C1A88654A\",\"description\":\"User is active in focus mode 30\\u0027 continues\",\"totalActions\":720,\"displayOrder\":2},{\"baseValue\":25,\"focustimeid\":\"124F4341-4C87-4E5F-BA4D-2481D8D36737\",\"description\":\"User is active in focus mode 45\\u0027 continues\",\"totalActions\":720,\"displayOrder\":3},{\"baseValue\":40,\"focustimeid\":\"82061293-C9D0-4598-99F1-8DFD8162B999\",\"description\":\"User is active in focus mode 60\\u0027 continues\",\"totalActions\":720,\"displayOrder\":4},{\"baseValue\":60,\"focustimeid\":\"FD7E66D3-A29B-4DA7-BB29-2283536D836A\",\"description\":\"User is active in focus mode 90\\u0027 continues\",\"totalActions\":720,\"displayOrder\":5}]");
        }
    }
}
