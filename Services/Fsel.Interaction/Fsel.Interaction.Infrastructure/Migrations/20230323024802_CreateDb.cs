using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Fsel.Interaction.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SurveyQuestions",
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
                    Question = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Icon = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AnswerStr = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SurveyQuestions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CustomerSurveys",
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
                    AnswerStr = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SurveyQuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerSurveys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerSurveys_SurveyQuestions_SurveyQuestionId",
                        column: x => x.SurveyQuestionId,
                        principalTable: "SurveyQuestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "SurveyQuestions",
                columns: new[] { "Id", "AnswerStr", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "Description", "DisplayOrder", "Icon", "IsDeleted", "Question", "Type", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[,]
                {
                    { new Guid("2be9a620-143d-41f6-815b-2038c21a7b23"), "{\"Birthday\":null,\"AgeGenderQuestions\":[{\"Id\":1,\"Gender\":0},{\"Id\":2,\"Gender\":1},{\"Id\":3,\"Gender\":2}]}", new DateTime(2023, 3, 23, 9, 48, 2, 368, DateTimeKind.Local).AddTicks(1989), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "addd", 1, "addd", false, "Xác định độ tuổi và giới tính", "AgeGender", null, null, null },
                    { new Guid("6410b229-32b8-457f-8556-92d93f27ef63"), "{\"ChooseLanguageQuestions\":[{\"Id\":1,\"Content\":\"V\\u0103n h\\u00F3a\",\"Image\":\"V\\u0103n h\\u00F3a\"},{\"Id\":2,\"Content\":\"Du l\\u1ECBch\",\"Image\":\"Du l\\u1ECBch\"},{\"Id\":3,\"Content\":\"K\\u1EBFt b\\u1EA1n v\\u00E0 chia s\\u1EBB\",\"Image\":\"K\\u1EBFt b\\u1EA1n v\\u00E0 chia s\\u1EBB\"},{\"Id\":4,\"Content\":\"H\\u1ECDc t\\u1EADp\",\"Image\":\"H\\u1ECDc t\\u1EADp\"},{\"Id\":5,\"Content\":\"C\\u01A1 h\\u1ED9i ngh\\u1EC1 nghi\\u1EC7p\",\"Image\":\"C\\u01A1 h\\u1ED9i ngh\\u1EC1 nghi\\u1EC7p\"},{\"Id\":6,\"Content\":\"Kh\\u00E1c....\",\"Image\":\"Kh\\u00E1c....\"}]}", new DateTime(2023, 3, 23, 9, 48, 2, 368, DateTimeKind.Local).AddTicks(1614), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "addd", 1, "addd", false, "Tại sao bạn học ngoại ngữ", "ChooseLanguage", null, null, null },
                    { new Guid("763d8bb9-cdbe-42e7-aa16-35a1915c3512"), "{\"FselSourceQuestions\":[{\"Id\":1,\"Content\":\"T\\u00ECm ki\\u1EBFm Google\",\"Image\":\"Google\"},{\"Id\":2,\"Content\":\"Facebook\",\"Image\":\"Facebook\"},{\"Id\":3,\"Content\":\"Youtube\",\"Image\":\"Youtube\"},{\"Id\":4,\"Content\":\"Tiktok\",\"Image\":\"Tiktok\"},{\"Id\":5,\"Content\":\"B\\u1EA1n b\\u00E8/Gia \\u0111\\u00ECnh\",\"Image\":\"B\\u1EA1n b\\u00E8/Gia \\u0111\\u00ECnh\"},{\"Id\":6,\"Content\":\"Tin t\\u1EE9c/B\\u00E1o ch\\u00ED/Blog\",\"Image\":\"Tin t\\u1EE9c/B\\u00E1o ch\\u00ED/Blog\"},{\"Id\":7,\"Content\":\"Tivi\",\"Image\":\"Tivi\"},{\"Id\":8,\"Content\":\"Kh\\u00E1c....\",\"Image\":\"Kh\\u00E1c....\"}]}", new DateTime(2023, 3, 23, 9, 48, 2, 368, DateTimeKind.Local).AddTicks(709), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "addd", 1, "addd", false, "Bạn biết đến Fsel từ đâu?", "FselSource", null, null, null },
                    { new Guid("96d16b4c-bb28-46b3-ad7d-e9c11beeeba9"), "{\"ChooseDirectionQuestions\":[{\"Id\":1,\"Content\":\"B\\u1EA1n \\u0111\\u00E3 bi\\u1EBFt m\\u1ED9t ch\\u00FAt Ti\\u1EBFng Anh\"},{\"Id\":2,\"Content\":\"\\u0110\\u00E2y l\\u00E0 l\\u1EA7n \\u0111\\u1EA7u b\\u1EA1n h\\u1ECDc Ti\\u1EBFng Anh\"}]}", new DateTime(2023, 3, 23, 9, 48, 2, 368, DateTimeKind.Local).AddTicks(1437), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "addd", 1, "addd", false, "Chọn hướng đi của bạn", "ChooseDirection", null, null, null },
                    { new Guid("adeaeda6-0e41-4ceb-a3fd-1b195317d776"), "{\"StudyTimeQuestions\":[{\"Id\":1,\"Content\":\"8 - 10 am\"},{\"Id\":2,\"Content\":\"1 - 3 pm\"},{\"Id\":3,\"Content\":\"3 - 5 pm\"},{\"Id\":4,\"Content\":\"5 - 9 pm\"},{\"Id\":5,\"Content\":\"7 - 10 pm\"}]}", new DateTime(2023, 3, 23, 9, 48, 2, 368, DateTimeKind.Local).AddTicks(1825), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "addd", 1, "addd", false, "Chọn thời gian học tập ", "StudyTime", null, null, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerSurveys_SurveyQuestionId",
                table: "CustomerSurveys",
                column: "SurveyQuestionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomerSurveys");

            migrationBuilder.DropTable(
                name: "SurveyQuestions");
        }
    }
}
