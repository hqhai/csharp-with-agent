using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Interaction.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSurveyQuestionV8Table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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
                keyValue: new Guid("f12f7c70-b9b5-49c5-8122-cc9c788e3354"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"H\\u1ECDc sinh, sinh vi\\u00EAn\",\"image\":\"student.svg\",\"referenceQuestionId\":\"b223125a-a4e1-4e10-b4dd-cfcd747d74c5\"},{\"id\":2,\"content\":\"Ng\\u01B0\\u1EDDi \\u0111i l\\u00E0m\",\"image\":\"worker.svg\",\"referenceQuestionId\":\"b223125a-a4e1-4e10-b4dd-cfcd747d74c5\"}]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("45c7789a-5eb5-491c-b518-24a2ce7f1dcf"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"Students\",\"image\":\"student.svg\"},{\"id\":2,\"content\":\"Working professionals\",\"image\":\"worker.svg\"}]");

            migrationBuilder.UpdateData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("f12f7c70-b9b5-49c5-8122-cc9c788e3354"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"H\\u1ECDc sinh, sinh vi\\u00EAn\",\"image\":\"student.svg\"},{\"id\":2,\"content\":\"Ng\\u01B0\\u1EDDi \\u0111i l\\u00E0m\",\"image\":\"worker.svg\"}]");
        }
    }
}
