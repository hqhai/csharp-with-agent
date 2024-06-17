using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Fsel.Interaction.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSurveyQuestionV6Table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("fd807c09-b8dc-49ba-9619-ad1a3220bc60"),
                column: "Question",
                value: "Why are studying a foreign language?");

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("2be9a620-143d-41f6-815b-2038c21a7b23"),
                column: "DisplayOrder",
                value: 7f);

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("492d8bb9-cdbe-42e7-aa16-35a1915c3621"),
                column: "DisplayOrder",
                value: 2f);

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("713d8bb9-cdbe-42e7-aa16-35a1915c3532"),
                column: "DisplayOrder",
                value: 5f);

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("721d8bb9-cdbe-42e7-aa16-35a1915c1123"),
                column: "DisplayOrder",
                value: 4f);

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("763d8bb9-cdbe-42e7-aa16-35a1915c3512"),
                column: "DisplayOrder",
                value: 3f);

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("b223125a-a4e1-4e10-b4dd-cfcd747d74c5"),
                column: "DisplayOrder",
                value: 6f);

            migrationBuilder.InsertData(
                table: "SurveyQuestions",
                columns: new[] { "Id", "AnswerStr", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "Description", "DisplayLevel", "DisplayOrder", "Icon", "IsDeleted", "IsPilot", "Question", "Type", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[] { new Guid("03d12e43-250b-49b7-bd08-b12135e47723"), "[{\"id\":1,\"content\":\"H\\u1ECDc sinh, sinh vi\\u00EAn\",\"image\":\"student.svg\"},{\"id\":2,\"content\":\"Ng\\u01B0\\u1EDDi \\u0111i l\\u00E0m\",\"image\":\"worker.svg\"}]", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "addd", 1, 1f, "addd", false, false, "Bạn là?", "ChooseMultipleColumn", null, null, null });

            migrationBuilder.InsertData(
                table: "SurveyQuestionTranslations",
                columns: new[] { "Id", "AnswerStr", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "Description", "IsDeleted", "Language", "Question", "SurveyQuestionId", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[,]
                {
                    { new Guid("45c7789a-5eb5-491c-b518-24a2ce7f1dcf"), "[{\"id\":1,\"content\":\"Students\",\"image\":\"student.svg\"},{\"id\":2,\"content\":\"Working professionals\",\"image\":\"worker.svg\"}]", new DateTime(2023, 10, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "addd", false, "en-US", "Who are you?", new Guid("03d12e43-250b-49b7-bd08-b12135e47723"), null, null, null },
                    { new Guid("f12f7c70-b9b5-49c5-8122-cc9c788e3354"), "[{\"id\":1,\"content\":\"H\\u1ECDc sinh, sinh vi\\u00EAn\",\"image\":\"student.svg\"},{\"id\":2,\"content\":\"Ng\\u01B0\\u1EDDi \\u0111i l\\u00E0m\",\"image\":\"worker.svg\"}]", new DateTime(2023, 10, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "addd", false, "vi-VN", "Bạn là?", new Guid("03d12e43-250b-49b7-bd08-b12135e47723"), null, null, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("45c7789a-5eb5-491c-b518-24a2ce7f1dcf"));

            migrationBuilder.DeleteData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("f12f7c70-b9b5-49c5-8122-cc9c788e3354"));

            migrationBuilder.DeleteData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("03d12e43-250b-49b7-bd08-b12135e47723"));

            migrationBuilder.UpdateData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("fd807c09-b8dc-49ba-9619-ad1a3220bc60"),
                column: "Question",
                value: "Why do you study foreign languages");

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("2be9a620-143d-41f6-815b-2038c21a7b23"),
                column: "DisplayOrder",
                value: 5f);

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("492d8bb9-cdbe-42e7-aa16-35a1915c3621"),
                column: "DisplayOrder",
                value: 1f);

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("713d8bb9-cdbe-42e7-aa16-35a1915c3532"),
                column: "DisplayOrder",
                value: 3f);

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("721d8bb9-cdbe-42e7-aa16-35a1915c1123"),
                column: "DisplayOrder",
                value: 2f);

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("763d8bb9-cdbe-42e7-aa16-35a1915c3512"),
                column: "DisplayOrder",
                value: 2f);

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("b223125a-a4e1-4e10-b4dd-cfcd747d74c5"),
                column: "DisplayOrder",
                value: 4f);
        }
    }
}
