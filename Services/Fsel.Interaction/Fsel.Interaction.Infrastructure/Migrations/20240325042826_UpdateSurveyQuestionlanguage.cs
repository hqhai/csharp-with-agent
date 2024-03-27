using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Fsel.Interaction.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSurveyQuestionlanguage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SurveyQuestionTranslations",
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
                    AnswerStr = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SurveyQuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Language = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SurveyQuestionTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SurveyQuestionTranslations_SurveyQuestions_SurveyQuestionId",
                        column: x => x.SurveyQuestionId,
                        principalTable: "SurveyQuestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "SurveyQuestionTranslations",
                columns: new[] { "Id", "AnswerStr", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "Description", "IsDeleted", "Language", "Question", "SurveyQuestionId", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[,]
                {
                    { new Guid("365375de-db86-4167-bbe8-e5a3ca2c154e"), "{\"country\":\"Other\",\"province\":\"Other\",\"district\":\"Other\",\"school\":\"Other\"}", new DateTime(2023, 10, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "addd", false, "vi-VN", "Trường học của bạn", new Guid("b223125a-a4e1-4e10-b4dd-cfcd747d74c5"), null, null, null },
                    { new Guid("4393a6a5-ff24-4d2d-b468-4b2c17ae0063"), "{\"country\":\"Other\",\"province\":\"Other\",\"district\":\"Other\",\"school\":\"Other\"}", new DateTime(2023, 10, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "addd", false, "en-US", "Your school", new Guid("b223125a-a4e1-4e10-b4dd-cfcd747d74c5"), null, null, null },
                    { new Guid("619b48dd-e305-4a8c-858d-fcbbdc980239"), "[{\"id\":1,\"content\":\"Google\",\"image\":\"gmail-icon.svg\"},{\"id\":2,\"content\":\"Facebook\",\"image\":\"facebook-icon.svg\"},{\"id\":3,\"content\":\"Youtube\",\"image\":\"youtube-icon.svg\"},{\"id\":4,\"content\":\"Tiktok\",\"image\":\"tiktok-icon.svg\"},{\"id\":5,\"content\":\"Friends/Family\",\"image\":\"friends_family-icon.svg\"},{\"id\":6,\"content\":\"News/Media\",\"image\":\"blog-icon.svg\"},{\"id\":7,\"content\":\"Tivi\",\"image\":\"tv-icon.svg\"},{\"id\":8,\"content\":\"Events/Conferences\",\"image\":\"event.svg\"},{\"id\":9,\"content\":\"School\",\"image\":\"school.svg\"},{\"id\":10,\"content\":\"Flyers\",\"image\":\"leaflets.svg\"},{\"id\":11,\"content\":\"Other....\",\"image\":\"others-icon.svg\"}]", new DateTime(2023, 10, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "addd", false, "en-US", "How did you know fsel? ", new Guid("492d8bb9-cdbe-42e7-aa16-35a1915c3621"), null, null, null },
                    { new Guid("7910a8a2-b89d-4579-a657-de2858ad499c"), "[{\"id\":1,\"content\":\"Google\",\"image\":\"gmail-icon.svg\"},{\"id\":2,\"content\":\"Facebook\",\"image\":\"facebook-icon.svg\"},{\"id\":3,\"content\":\"Youtube\",\"image\":\"youtube-icon.svg\"},{\"id\":4,\"content\":\"Tiktok\",\"image\":\"tiktok-icon.svg\"},{\"id\":5,\"content\":\"B\\u1EA1n b\\u00E8/gia \\u0111\\u00ECnh\",\"image\":\"friends_family-icon.svg\"},{\"id\":6,\"content\":\"Tin t\\u1EE9c/b\\u00E1o ch\\u00ED\",\"image\":\"blog-icon.svg\"},{\"id\":7,\"content\":\"Tivi\",\"image\":\"tv-icon.svg\"},{\"id\":8,\"content\":\"S\\u1EF1 ki\\u1EC7n/h\\u1ED9i th\\u1EA3o\",\"image\":\"event.svg\"},{\"id\":9,\"content\":\"Tr\\u01B0\\u1EDDng h\\u1ECDc\",\"image\":\"school.svg\"},{\"id\":10,\"content\":\"T\\u1EDD r\\u01A1i\",\"image\":\"leaflets.svg\"},{\"id\":11,\"content\":\"Kh\\u00E1c....\",\"image\":\"others-icon.svg\"}]", new DateTime(2023, 10, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "addd", false, "vi-VN", "Bạn biết đến Fsel từ đâu?", new Guid("492d8bb9-cdbe-42e7-aa16-35a1915c3621"), null, null, null },
                    { new Guid("b695a761-d0fa-4162-97c3-68403e9a8326"), "{\"birthday\":null,\"ageGenderQuestions\":[{\"id\":1,\"content\":\"Male\"},{\"id\":2,\"content\":\"Female\"},{\"id\":3,\"content\":\"Other\"}]}", new DateTime(2023, 10, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "addd", false, "vi-VN", "Xác định độ tuổi và giới tính", new Guid("2be9a620-143d-41f6-815b-2038c21a7b23"), null, null, null },
                    { new Guid("ba027423-106a-4bc6-a3a6-4b8386a44e51"), "[{\"id\":1,\"content\":\"V\\u0103n h\\u00F3a\",\"image\":\"castle 1.png\"},{\"id\":2,\"content\":\"Du l\\u1ECBch\",\"image\":\"plane 1.png\"},{\"id\":3,\"content\":\"K\\u1EBFt b\\u1EA1n\",\"image\":\"friendship 1.png\"},{\"id\":4,\"content\":\"H\\u1ECDc t\\u1EADp\",\"image\":\"mortarboard 1.png\"},{\"id\":5,\"content\":\"C\\u01A1 h\\u1ED9i ngh\\u1EC1 nghi\\u1EC7p\",\"image\":\"case 1.png\"},{\"id\":6,\"content\":\"Kh\\u00E1c....\",\"image\":\"goal 1.png\"}]", new DateTime(2023, 10, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "addd", false, "vi-VN", "Tại sao bạn học ngoại ngữ", new Guid("721d8bb9-cdbe-42e7-aa16-35a1915c1123"), null, null, null },
                    { new Guid("e343b238-f2fc-418a-a171-6cce3d90d2a1"), "{\"birthday\":null,\"ageGenderQuestions\":[{\"id\":1,\"content\":\"Male\"},{\"id\":2,\"content\":\"Female\"},{\"id\":3,\"content\":\"Other\"}]}", new DateTime(2023, 10, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "addd", false, "en-US", "age and gender", new Guid("2be9a620-143d-41f6-815b-2038c21a7b23"), null, null, null },
                    { new Guid("fd807c09-b8dc-49ba-9619-ad1a3220bc60"), "[{\"id\":1,\"content\":\"Culture\",\"image\":\"castle 1.png\"},{\"id\":2,\"content\":\"Travel\",\"image\":\"plane 1.png\"},{\"id\":3,\"content\":\"Making friends\",\"image\":\"friendship 1.png\"},{\"id\":4,\"content\":\"Education\",\"image\":\"mortarboard 1.png\"},{\"id\":5,\"content\":\"Career opportunities\",\"image\":\"case 1.png\"},{\"id\":6,\"content\":\"Other....\",\"image\":\"goal 1.png\"}]", new DateTime(2023, 10, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "addd", false, "en-US", "Why do you study foreign languages", new Guid("721d8bb9-cdbe-42e7-aa16-35a1915c1123"), null, null, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_SurveyQuestionTranslations_SurveyQuestionId",
                table: "SurveyQuestionTranslations",
                column: "SurveyQuestionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SurveyQuestionTranslations");
        }
    }
}
