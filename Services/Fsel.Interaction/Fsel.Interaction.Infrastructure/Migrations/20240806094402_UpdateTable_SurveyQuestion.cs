using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Fsel.Interaction.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTable_SurveyQuestion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsCompleted",
                table: "CustomerSurveys",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("03d12e43-250b-49b7-bd08-b12135e47723"),
                column: "DisplayOrder",
                value: 2f);

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("2be9a620-143d-41f6-815b-2038c21a7b23"),
                column: "IsPilot",
                value: null);

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("35a71ae7-49c1-4878-a1db-edcd2834f1cd"),
                column: "DisplayOrder",
                value: 4f);

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("492d8bb9-cdbe-42e7-aa16-35a1915c3621"),
                columns: new[] { "DisplayOrder", "IsPilot" },
                values: new object[] { 1f, null });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("721d8bb9-cdbe-42e7-aa16-35a1915c1123"),
                column: "IsPilot",
                value: null);

            migrationBuilder.InsertData(
                table: "SurveyQuestions",
                columns: new[] { "Id", "AnswerStr", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "Description", "DisplayLevel", "DisplayOrder", "Icon", "IsDeleted", "IsPilot", "Question", "Type", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[,]
                {
                    { new Guid("14787cbf-cc43-4453-a148-6d11a683f311"), "[{\"id\":1,\"content\":\"N\\u00E2ng cao \\u0111i\\u1EC3m s\\u1ED1\",\"image\":\"goal 1.png\"},{\"id\":2,\"content\":\"M\\u1EE5c ti\\u00EAu c\\u00F4ng vi\\u1EC7c\",\"image\":\"work-target-icon.svg\"},{\"id\":3,\"content\":\"C\\u1EA3i thi\\u1EC7n giao ti\\u1EBFp\",\"image\":\"friendship 1.png\"},{\"id\":4,\"content\":\"Thi ch\\u1EE9ng ch\\u1EC9\",\"image\":\"student.svg\"},{\"id\":5,\"content\":\"Du h\\u1ECDc\",\"image\":\"plane 1.png\"}]", new DateTime(2024, 8, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Câu hỏi mục tiêu", 1, 3f, "addd", false, false, "Mục tiêu học tập của bạn là gì?", "CheckList", null, null, null },
                    { new Guid("f563da40-d609-4922-90b9-44e4290edfef"), "[{\"id\":1,\"content\":\"Google\",\"image\":\"gmail-icon.svg\"},{\"id\":2,\"content\":\"Facebook\",\"image\":\"facebook-icon.svg\"},{\"id\":3,\"content\":\"YouTube\",\"image\":\"youtube-icon.svg\"},{\"id\":4,\"content\":\"Tiktok\",\"image\":\"tiktok-icon.svg\"},{\"id\":5,\"content\":\"B\\u1EA1n b\\u00E8/gia \\u0111\\u00ECnh\",\"image\":\"friends_family-icon.svg\"},{\"id\":6,\"content\":\"Tin t\\u1EE9c/b\\u00E1o ch\\u00ED\",\"image\":\"blog-icon.svg\"},{\"id\":7,\"content\":\"TV\",\"image\":\"tv-icon.svg\"},{\"id\":8,\"content\":\"S\\u1EF1 ki\\u1EC7n/h\\u1ED9i th\\u1EA3o\",\"image\":\"event.svg\"},{\"id\":9,\"content\":\"Tr\\u01B0\\u1EDDng h\\u1ECDc\",\"image\":\"school.svg\"},{\"id\":10,\"content\":\"T\\u1EDD r\\u01A1i\",\"image\":\"leaflets.svg\"},{\"id\":11,\"content\":\"Kh\\u00E1c....\",\"image\":\"others-icon.svg\"}]", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Câu hỏi biết đến Fsel", 1, 1f, "addd", false, false, "Bạn biết đến Fsel từ đâu?", "CheckList", null, null, null }
                });

            migrationBuilder.InsertData(
                table: "SurveyQuestionTranslations",
                columns: new[] { "Id", "AnswerStr", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "Description", "IsDeleted", "Language", "Question", "SurveyQuestionId", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[,]
                {
                    { new Guid("2b267643-bb4f-4ac4-a14a-33350fa35108"), "[{\"id\":1,\"content\":\"Google\",\"image\":\"gmail-icon.svg\"},{\"id\":2,\"content\":\"Facebook\",\"image\":\"facebook-icon.svg\"},{\"id\":3,\"content\":\"YouTube\",\"image\":\"youtube-icon.svg\"},{\"id\":4,\"content\":\"Tiktok\",\"image\":\"tiktok-icon.svg\"},{\"id\":5,\"content\":\"B\\u1EA1n b\\u00E8/gia \\u0111\\u00ECnh\",\"image\":\"friends_family-icon.svg\"},{\"id\":6,\"content\":\"Tin t\\u1EE9c/b\\u00E1o ch\\u00ED\",\"image\":\"blog-icon.svg\"},{\"id\":7,\"content\":\"TV\",\"image\":\"tv-icon.svg\"},{\"id\":8,\"content\":\"S\\u1EF1 ki\\u1EC7n/h\\u1ED9i th\\u1EA3o\",\"image\":\"event.svg\"},{\"id\":9,\"content\":\"Tr\\u01B0\\u1EDDng h\\u1ECDc\",\"image\":\"school.svg\"},{\"id\":10,\"content\":\"T\\u1EDD r\\u01A1i\",\"image\":\"leaflets.svg\"},{\"id\":11,\"content\":\"Kh\\u00E1c....\",\"image\":\"others-icon.svg\"}]", new DateTime(2023, 10, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "addd", false, "vi-VN", "Bạn biết đến Fsel từ đâu?", new Guid("f563da40-d609-4922-90b9-44e4290edfef"), null, null, null },
                    { new Guid("877fc41d-d1ab-4285-8349-d8c71b9ea0a2"), "[{\"id\":1,\"content\":\"Improve scores\",\"image\":\"goal 1.png\"},{\"id\":2,\"content\":\"Intended use for work\",\"image\":\"work-target-icon.svg\"},{\"id\":3,\"content\":\"Improve communication\",\"image\":\"friendship 1.png\"},{\"id\":4,\"content\":\"Certificate exam\",\"image\":\"student.svg\"},{\"id\":5,\"content\":\"Oversea\",\"image\":\"plane 1.png\"}]", new DateTime(2024, 8, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Ask for target", false, "en-US", "What are your learning goals?", new Guid("14787cbf-cc43-4453-a148-6d11a683f311"), null, null, null },
                    { new Guid("d823acb4-62d8-4702-b95c-e6c5a8e49e4e"), "[{\"id\":1,\"content\":\"N\\u00E2ng cao \\u0111i\\u1EC3m s\\u1ED1\",\"image\":\"goal 1.png\"},{\"id\":2,\"content\":\"M\\u1EE5c ti\\u00EAu c\\u00F4ng vi\\u1EC7c\",\"image\":\"work-target-icon.svg\"},{\"id\":3,\"content\":\"C\\u1EA3i thi\\u1EC7n giao ti\\u1EBFp\",\"image\":\"friendship 1.png\"},{\"id\":4,\"content\":\"Thi ch\\u1EE9ng ch\\u1EC9\",\"image\":\"student.svg\"},{\"id\":5,\"content\":\"Du h\\u1ECDc\",\"image\":\"plane 1.png\"}]", new DateTime(2024, 8, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Câu hỏi mục tiêu", false, "vi-VN", "Mục tiêu học tập của bạn là gì?", new Guid("14787cbf-cc43-4453-a148-6d11a683f311"), null, null, null },
                    { new Guid("f601e0f3-90c2-4738-8e07-cd5120e39148"), "[{\"id\":1,\"content\":\"Google\",\"image\":\"gmail-icon.svg\"},{\"id\":2,\"content\":\"Facebook\",\"image\":\"facebook-icon.svg\"},{\"id\":3,\"content\":\"YouTube\",\"image\":\"youtube-icon.svg\"},{\"id\":4,\"content\":\"Tiktok\",\"image\":\"tiktok-icon.svg\"},{\"id\":5,\"content\":\"Friends/Family\",\"image\":\"friends_family-icon.svg\"},{\"id\":6,\"content\":\"News/Media\",\"image\":\"blog-icon.svg\"},{\"id\":7,\"content\":\"TV\",\"image\":\"tv-icon.svg\"},{\"id\":8,\"content\":\"Events/Conferences\",\"image\":\"event.svg\"},{\"id\":9,\"content\":\"School\",\"image\":\"school.svg\"},{\"id\":10,\"content\":\"Flyers\",\"image\":\"leaflets.svg\"},{\"id\":11,\"content\":\"Other....\",\"image\":\"others-icon.svg\"}]", new DateTime(2023, 10, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "addd", false, "en-US", "How do you know FSEL?", new Guid("f563da40-d609-4922-90b9-44e4290edfef"), null, null, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("2b267643-bb4f-4ac4-a14a-33350fa35108"));

            migrationBuilder.DeleteData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("877fc41d-d1ab-4285-8349-d8c71b9ea0a2"));

            migrationBuilder.DeleteData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("d823acb4-62d8-4702-b95c-e6c5a8e49e4e"));

            migrationBuilder.DeleteData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("f601e0f3-90c2-4738-8e07-cd5120e39148"));

            migrationBuilder.DeleteData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("14787cbf-cc43-4453-a148-6d11a683f311"));

            migrationBuilder.DeleteData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("f563da40-d609-4922-90b9-44e4290edfef"));

            migrationBuilder.DropColumn(
                name: "IsCompleted",
                table: "CustomerSurveys");

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("03d12e43-250b-49b7-bd08-b12135e47723"),
                column: "DisplayOrder",
                value: 1f);

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("2be9a620-143d-41f6-815b-2038c21a7b23"),
                column: "IsPilot",
                value: false);

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("35a71ae7-49c1-4878-a1db-edcd2834f1cd"),
                column: "DisplayOrder",
                value: 6f);

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("492d8bb9-cdbe-42e7-aa16-35a1915c3621"),
                columns: new[] { "DisplayOrder", "IsPilot" },
                values: new object[] { 2f, false });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("721d8bb9-cdbe-42e7-aa16-35a1915c1123"),
                column: "IsPilot",
                value: false);
        }
    }
}
