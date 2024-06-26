using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Interaction.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSurveyQuestionV9Table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("45c7789a-5eb5-491c-b518-24a2ce7f1dcf"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"Students\",\"image\":\"student.svg\",\"referenceQuestionId\":\"b223125a-a4e1-4e10-b4dd-cfcd747d74c5\"},{\"id\":2,\"content\":\"Working professionals\",\"image\":\"worker.svg\",\"referenceQuestionId\":\"35a71ae7-49c1-4878-a1db-edcd2834f1cd\"}]");

            migrationBuilder.UpdateData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("619b48dd-e305-4a8c-858d-fcbbdc980239"),
                columns: new[] { "AnswerStr", "Question" },
                values: new object[] { "[{\"id\":1,\"content\":\"Google\",\"image\":\"gmail-icon.svg\"},{\"id\":2,\"content\":\"Facebook\",\"image\":\"facebook-icon.svg\"},{\"id\":3,\"content\":\"YouTube\",\"image\":\"youtube-icon.svg\"},{\"id\":4,\"content\":\"Tiktok\",\"image\":\"tiktok-icon.svg\"},{\"id\":5,\"content\":\"Friends/Family\",\"image\":\"friends_family-icon.svg\"},{\"id\":6,\"content\":\"News/Media\",\"image\":\"blog-icon.svg\"},{\"id\":7,\"content\":\"TV\",\"image\":\"tv-icon.svg\"},{\"id\":8,\"content\":\"Events/Conferences\",\"image\":\"event.svg\"},{\"id\":9,\"content\":\"School\",\"image\":\"school.svg\"},{\"id\":10,\"content\":\"Flyers\",\"image\":\"leaflets.svg\"},{\"id\":11,\"content\":\"Other....\",\"image\":\"others-icon.svg\"}]", "How do you know FSEL?" });

            migrationBuilder.UpdateData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("7910a8a2-b89d-4579-a657-de2858ad499c"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"Google\",\"image\":\"gmail-icon.svg\"},{\"id\":2,\"content\":\"Facebook\",\"image\":\"facebook-icon.svg\"},{\"id\":3,\"content\":\"YouTube\",\"image\":\"youtube-icon.svg\"},{\"id\":4,\"content\":\"Tiktok\",\"image\":\"tiktok-icon.svg\"},{\"id\":5,\"content\":\"B\\u1EA1n b\\u00E8/gia \\u0111\\u00ECnh\",\"image\":\"friends_family-icon.svg\"},{\"id\":6,\"content\":\"Tin t\\u1EE9c/b\\u00E1o ch\\u00ED\",\"image\":\"blog-icon.svg\"},{\"id\":7,\"content\":\"TV\",\"image\":\"tv-icon.svg\"},{\"id\":8,\"content\":\"S\\u1EF1 ki\\u1EC7n/h\\u1ED9i th\\u1EA3o\",\"image\":\"event.svg\"},{\"id\":9,\"content\":\"Tr\\u01B0\\u1EDDng h\\u1ECDc\",\"image\":\"school.svg\"},{\"id\":10,\"content\":\"T\\u1EDD r\\u01A1i\",\"image\":\"leaflets.svg\"},{\"id\":11,\"content\":\"Kh\\u00E1c....\",\"image\":\"others-icon.svg\"}]");

            migrationBuilder.UpdateData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("f12f7c70-b9b5-49c5-8122-cc9c788e3354"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"H\\u1ECDc sinh, sinh vi\\u00EAn\",\"image\":\"student.svg\",\"referenceQuestionId\":\"b223125a-a4e1-4e10-b4dd-cfcd747d74c5\"},{\"id\":2,\"content\":\"Ng\\u01B0\\u1EDDi \\u0111i l\\u00E0m\",\"image\":\"worker.svg\",\"referenceQuestionId\":\"35a71ae7-49c1-4878-a1db-edcd2834f1cd\"}]");

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("492d8bb9-cdbe-42e7-aa16-35a1915c3621"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"Google\",\"image\":\"gmail-icon.svg\"},{\"id\":2,\"content\":\"Facebook\",\"image\":\"facebook-icon.svg\"},{\"id\":3,\"content\":\"YouTube\",\"image\":\"youtube-icon.svg\"},{\"id\":4,\"content\":\"Tiktok\",\"image\":\"tiktok-icon.svg\"},{\"id\":5,\"content\":\"B\\u1EA1n b\\u00E8/gia \\u0111\\u00ECnh\",\"image\":\"friends_family-icon.svg\"},{\"id\":6,\"content\":\"Tin t\\u1EE9c/b\\u00E1o ch\\u00ED\",\"image\":\"blog-icon.svg\"},{\"id\":7,\"content\":\"TV\",\"image\":\"tv-icon.svg\"},{\"id\":8,\"content\":\"S\\u1EF1 ki\\u1EC7n/h\\u1ED9i th\\u1EA3o\",\"image\":\"event.svg\"},{\"id\":9,\"content\":\"Tr\\u01B0\\u1EDDng h\\u1ECDc\",\"image\":\"school.svg\"},{\"id\":10,\"content\":\"T\\u1EDD r\\u01A1i\",\"image\":\"leaflets.svg\"},{\"id\":11,\"content\":\"Kh\\u00E1c....\",\"image\":\"others-icon.svg\"}]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("45c7789a-5eb5-491c-b518-24a2ce7f1dcf"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"Students\",\"image\":\"student.svg\",\"referenceQuestionId\":\"b223125a-a4e1-4e10-b4dd-cfcd747d74c5\"},{\"id\":2,\"content\":\"Working professionals\",\"image\":\"worker.svg\",\"referenceQuestionId\":\"b223125a-a4e1-4e10-b4dd-cfcd747d74c5\"}]");

            migrationBuilder.UpdateData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("619b48dd-e305-4a8c-858d-fcbbdc980239"),
                columns: new[] { "AnswerStr", "Question" },
                values: new object[] { "[{\"id\":1,\"content\":\"Google\",\"image\":\"gmail-icon.svg\"},{\"id\":2,\"content\":\"Facebook\",\"image\":\"facebook-icon.svg\"},{\"id\":3,\"content\":\"Youtube\",\"image\":\"youtube-icon.svg\"},{\"id\":4,\"content\":\"Tiktok\",\"image\":\"tiktok-icon.svg\"},{\"id\":5,\"content\":\"Friends/Family\",\"image\":\"friends_family-icon.svg\"},{\"id\":6,\"content\":\"News/Media\",\"image\":\"blog-icon.svg\"},{\"id\":7,\"content\":\"Tivi\",\"image\":\"tv-icon.svg\"},{\"id\":8,\"content\":\"Events/Conferences\",\"image\":\"event.svg\"},{\"id\":9,\"content\":\"School\",\"image\":\"school.svg\"},{\"id\":10,\"content\":\"Flyers\",\"image\":\"leaflets.svg\"},{\"id\":11,\"content\":\"Other....\",\"image\":\"others-icon.svg\"}]", "How did you know fsel? " });

            migrationBuilder.UpdateData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("7910a8a2-b89d-4579-a657-de2858ad499c"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"Google\",\"image\":\"gmail-icon.svg\"},{\"id\":2,\"content\":\"Facebook\",\"image\":\"facebook-icon.svg\"},{\"id\":3,\"content\":\"Youtube\",\"image\":\"youtube-icon.svg\"},{\"id\":4,\"content\":\"Tiktok\",\"image\":\"tiktok-icon.svg\"},{\"id\":5,\"content\":\"B\\u1EA1n b\\u00E8/gia \\u0111\\u00ECnh\",\"image\":\"friends_family-icon.svg\"},{\"id\":6,\"content\":\"Tin t\\u1EE9c/b\\u00E1o ch\\u00ED\",\"image\":\"blog-icon.svg\"},{\"id\":7,\"content\":\"Tivi\",\"image\":\"tv-icon.svg\"},{\"id\":8,\"content\":\"S\\u1EF1 ki\\u1EC7n/h\\u1ED9i th\\u1EA3o\",\"image\":\"event.svg\"},{\"id\":9,\"content\":\"Tr\\u01B0\\u1EDDng h\\u1ECDc\",\"image\":\"school.svg\"},{\"id\":10,\"content\":\"T\\u1EDD r\\u01A1i\",\"image\":\"leaflets.svg\"},{\"id\":11,\"content\":\"Kh\\u00E1c....\",\"image\":\"others-icon.svg\"}]");

            migrationBuilder.UpdateData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("f12f7c70-b9b5-49c5-8122-cc9c788e3354"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"H\\u1ECDc sinh, sinh vi\\u00EAn\",\"image\":\"student.svg\",\"referenceQuestionId\":\"b223125a-a4e1-4e10-b4dd-cfcd747d74c5\"},{\"id\":2,\"content\":\"Ng\\u01B0\\u1EDDi \\u0111i l\\u00E0m\",\"image\":\"worker.svg\",\"referenceQuestionId\":\"b223125a-a4e1-4e10-b4dd-cfcd747d74c5\"}]");

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("492d8bb9-cdbe-42e7-aa16-35a1915c3621"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"Google\",\"image\":\"gmail-icon.svg\"},{\"id\":2,\"content\":\"Facebook\",\"image\":\"facebook-icon.svg\"},{\"id\":3,\"content\":\"Youtube\",\"image\":\"youtube-icon.svg\"},{\"id\":4,\"content\":\"Tiktok\",\"image\":\"tiktok-icon.svg\"},{\"id\":5,\"content\":\"B\\u1EA1n b\\u00E8/gia \\u0111\\u00ECnh\",\"image\":\"friends_family-icon.svg\"},{\"id\":6,\"content\":\"Tin t\\u1EE9c/b\\u00E1o ch\\u00ED\",\"image\":\"blog-icon.svg\"},{\"id\":7,\"content\":\"Tivi\",\"image\":\"tv-icon.svg\"},{\"id\":8,\"content\":\"S\\u1EF1 ki\\u1EC7n/h\\u1ED9i th\\u1EA3o\",\"image\":\"event.svg\"},{\"id\":9,\"content\":\"Tr\\u01B0\\u1EDDng h\\u1ECDc\",\"image\":\"school.svg\"},{\"id\":10,\"content\":\"T\\u1EDD r\\u01A1i\",\"image\":\"leaflets.svg\"},{\"id\":11,\"content\":\"Kh\\u00E1c....\",\"image\":\"others-icon.svg\"}]");
        }
    }
}
