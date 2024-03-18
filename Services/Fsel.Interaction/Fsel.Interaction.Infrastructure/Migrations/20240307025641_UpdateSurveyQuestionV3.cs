using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Interaction.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSurveyQuestionV3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("492d8bb9-cdbe-42e7-aa16-35a1915c3621"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"Google\",\"image\":\"gmail-icon.svg\"},{\"id\":2,\"content\":\"Facebook\",\"image\":\"facebook-icon.svg\"},{\"id\":3,\"content\":\"Youtube\",\"image\":\"youtube-icon.svg\"},{\"id\":4,\"content\":\"Tiktok\",\"image\":\"tiktok-icon.svg\"},{\"id\":5,\"content\":\"B\\u1EA1n b\\u00E8/gia \\u0111\\u00ECnh\",\"image\":\"friends_family-icon.svg\"},{\"id\":6,\"content\":\"Tin t\\u1EE9c/b\\u00E1o ch\\u00ED\",\"image\":\"blog-icon.svg\"},{\"id\":7,\"content\":\"Tivi\",\"image\":\"tv-icon.svg\"},{\"id\":8,\"content\":\"Kh\\u00E1c....\",\"image\":\"others-icon.svg\"}]");

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("721d8bb9-cdbe-42e7-aa16-35a1915c1123"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"V\\u0103n h\\u00F3a\",\"image\":\"castle 1.png\"},{\"id\":2,\"content\":\"Du l\\u1ECBch\",\"image\":\"plane 1.png\"},{\"id\":3,\"content\":\"K\\u1EBFt b\\u1EA1n\",\"image\":\"friendship 1.png\"},{\"id\":4,\"content\":\"H\\u1ECDc t\\u1EADp\",\"image\":\"mortarboard 1.png\"},{\"id\":5,\"content\":\"C\\u01A1 h\\u1ED9i ngh\\u1EC1 nghi\\u1EC7p\",\"image\":\"case 1.png\"},{\"id\":6,\"content\":\"Kh\\u00E1c....\",\"image\":\"goal 1.png\"}]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("492d8bb9-cdbe-42e7-aa16-35a1915c3621"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"T\\u00ECm ki\\u1EBFm Google\",\"image\":\"gmail-icon.svg\"},{\"id\":2,\"content\":\"Facebook\",\"image\":\"facebook-icon.svg\"},{\"id\":3,\"content\":\"Youtube\",\"image\":\"youtube-icon.svg\"},{\"id\":4,\"content\":\"Tiktok\",\"image\":\"tiktok-icon.svg\"},{\"id\":5,\"content\":\"B\\u1EA1n b\\u00E8/Gia \\u0111\\u00ECnh\",\"image\":\"friends_family-icon.svg\"},{\"id\":6,\"content\":\"Tin t\\u1EE9c/B\\u00E1o ch\\u00ED/Blog\",\"image\":\"blog-icon.svg\"},{\"id\":7,\"content\":\"Tivi\",\"image\":\"tv-icon.svg\"},{\"id\":8,\"content\":\"Kh\\u00E1c....\",\"image\":\"others-icon.svg\"}]");

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("721d8bb9-cdbe-42e7-aa16-35a1915c1123"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"V\\u0103n h\\u00F3a\",\"image\":\"castle 1.png\"},{\"id\":2,\"content\":\"Du l\\u1ECBch\",\"image\":\"plane 1.png\"},{\"id\":3,\"content\":\"K\\u1EBFt b\\u1EA1n v\\u00E0 chia s\\u1EBB\",\"image\":\"friendship 1.png\"},{\"id\":4,\"content\":\"H\\u1ECDc t\\u1EADp\",\"image\":\"mortarboard 1.png\"},{\"id\":5,\"content\":\"C\\u01A1 h\\u1ED9i ngh\\u1EC1 nghi\\u1EC7p\",\"image\":\"case 1.png\"},{\"id\":6,\"content\":\"Kh\\u00E1c....\",\"image\":\"goal 1.png\"}]");
        }
    }
}
