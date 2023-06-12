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
                name: "Comments",
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
                    Content = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ObjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InteractionActions",
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
                    Type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ObjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InteractionActions", x => x.Id);
                });

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
                    AnswerStr = table.Column<string>(type: "nvarchar(max)", nullable: false)
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
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SurveyQuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerSurveys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerSurveys_SurveyQuestions_SurveyQuestionId",
                        column: x => x.SurveyQuestionId,
                        principalTable: "SurveyQuestions",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "SurveyQuestions",
                columns: new[] { "Id", "AnswerStr", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "Description", "DisplayOrder", "Icon", "IsDeleted", "Question", "Type", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[,]
                {
                    { new Guid("2be9a620-143d-41f6-815b-2038c21a7b23"), "{\"birthday\":null,\"ageGenderQuestions\":[{\"id\":1,\"gender\":\"Male\"},{\"id\":2,\"gender\":\"Female\"},{\"id\":3,\"gender\":\"Other\"}]}", new DateTime(2023, 6, 12, 21, 31, 11, 310, DateTimeKind.Local).AddTicks(330), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "addd", 5, "time.png", false, "Xác định độ tuổi và giới tính", "AgeGender", null, null, null },
                    { new Guid("492d8bb9-cdbe-42e7-aa16-35a1915c3621"), "[{\"id\":1,\"content\":\"T\\u00ECm ki\\u1EBFm Google\",\"image\":\"gmail-icon.png\"},{\"id\":2,\"content\":\"Facebook\",\"image\":\"facebook-icon.png\"},{\"id\":3,\"content\":\"Youtube\",\"image\":\"youtube-icon.png\"},{\"id\":4,\"content\":\"Tiktok\",\"image\":\"tiktok-icon.png\"},{\"id\":5,\"content\":\"B\\u1EA1n b\\u00E8/Gia \\u0111\\u00ECnh\",\"image\":\"friends_family-icon.svg\"},{\"id\":6,\"content\":\"Tin t\\u1EE9c/B\\u00E1o ch\\u00ED/Blog\",\"image\":\"blog-icon.svg\"},{\"id\":7,\"content\":\"Tivi\",\"image\":\"tv-icon.svg\"},{\"id\":8,\"content\":\"Kh\\u00E1c....\",\"image\":\"others-icon.png\"}]", new DateTime(2023, 6, 12, 21, 31, 11, 306, DateTimeKind.Local).AddTicks(2084), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "addd", 1, "addd", false, "Bạn biết đến Fsel từ đâu?", "ChooseMultipleColumn", null, null, null },
                    { new Guid("713d8bb9-cdbe-42e7-aa16-35a1915c3532"), "{\"countryCode\":123,\"countryName\":\"Vi\\u1EC7t Nam\",\"provinceCode\":29,\"provinceName\":\"H\\u00E0 N\\u1ED9i\"}", new DateTime(2023, 6, 12, 21, 31, 11, 309, DateTimeKind.Local).AddTicks(9696), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "addd", 4, "addd", false, "Vị trí của bạn", "YourPlace", null, null, null },
                    { new Guid("721d8bb9-cdbe-42e7-aa16-35a1915c1123"), "[{\"id\":1,\"content\":\"V\\u0103n h\\u00F3a\",\"image\":\"castle 1.png\"},{\"id\":2,\"content\":\"Du l\\u1ECBch\",\"image\":\"plane 1.png\"},{\"id\":3,\"content\":\"K\\u1EBFt b\\u1EA1n v\\u00E0 chia s\\u1EBB\",\"image\":\"friendship 1.png\"},{\"id\":4,\"content\":\"H\\u1ECDc t\\u1EADp\",\"image\":\"mortarboard 1.png\"},{\"id\":5,\"content\":\"C\\u01A1 h\\u1ED9i ngh\\u1EC1 nghi\\u1EC7p\",\"image\":\"case 1.png\"},{\"id\":6,\"content\":\"Kh\\u00E1c....\",\"image\":\"goal 1.png\"}]", new DateTime(2023, 6, 12, 21, 31, 11, 309, DateTimeKind.Local).AddTicks(8377), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "addd", 3, "addd", false, "Tại sao bạn học ngoại ngữ", "ChooseMultipleColumn", null, null, null },
                    { new Guid("763d8bb9-cdbe-42e7-aa16-35a1915c3512"), "[{\"id\":1,\"content\":\"B\\u1EA1n \\u0111\\u00E3 bi\\u1EBFt m\\u1ED9t ch\\u00FAt Ti\\u1EBFng Anh\"},{\"id\":2,\"content\":\"\\u0110\\u00E2y l\\u00E0 l\\u1EA7n \\u0111\\u1EA7u b\\u1EA1n h\\u1ECDc Ti\\u1EBFng Anh\"}]", new DateTime(2023, 6, 12, 21, 31, 11, 309, DateTimeKind.Local).AddTicks(5198), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "addd", 2, "addd", false, "Chọn hướng đi của bạn", "YourDirection", null, null, null }
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
                name: "Comments");

            migrationBuilder.DropTable(
                name: "CustomerSurveys");

            migrationBuilder.DropTable(
                name: "InteractionActions");

            migrationBuilder.DropTable(
                name: "SurveyQuestions");
        }
    }
}
