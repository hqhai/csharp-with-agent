using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Fsel.Interaction.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSurveyQuestionV7Table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("03d12e43-250b-49b7-bd08-b12135e47723"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"H\\u1ECDc sinh, sinh vi\\u00EAn\",\"image\":\"student.svg\",\"referenceQuestionId\":\"b223125a-a4e1-4e10-b4dd-cfcd747d74c5\"},{\"id\":2,\"content\":\"Ng\\u01B0\\u1EDDi \\u0111i l\\u00E0m\",\"image\":\"worker.svg\",\"referenceQuestionId\":\"35a71ae7-49c1-4878-a1db-edcd2834f1cd\"}]");

            migrationBuilder.InsertData(
                table: "SurveyQuestions",
                columns: new[] { "Id", "AnswerStr", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "Description", "DisplayLevel", "DisplayOrder", "Icon", "IsDeleted", "IsPilot", "Question", "Type", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[] { new Guid("35a71ae7-49c1-4878-a1db-edcd2834f1cd"), "{\"country\":\"Other\",\"province\":\"Other\",\"district\":\"Other\"}", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "addd", 1, 6f, "wideword.png", false, false, "Vị trí của bạn?", "Location", null, null, null });

            migrationBuilder.InsertData(
                table: "SurveyQuestionTranslations",
                columns: new[] { "Id", "AnswerStr", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "Description", "IsDeleted", "Language", "Question", "SurveyQuestionId", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[,]
                {
                    { new Guid("b606fff3-5363-4d82-a798-7059e9323321"), "{\"country\":\"Other\",\"province\":\"Other\",\"district\":\"Other\"}", new DateTime(2023, 10, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "addd", false, "vi-VN", "Vị trí của bạn?", new Guid("35a71ae7-49c1-4878-a1db-edcd2834f1cd"), null, null, null },
                    { new Guid("d9343200-7b1b-475c-8330-a8d3e1a09be4"), "{\"country\":\"Other\",\"province\":\"Other\",\"district\":\"Other\"}", new DateTime(2023, 10, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "addd", false, "en-US", "Your position?", new Guid("35a71ae7-49c1-4878-a1db-edcd2834f1cd"), null, null, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("b606fff3-5363-4d82-a798-7059e9323321"));

            migrationBuilder.DeleteData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("d9343200-7b1b-475c-8330-a8d3e1a09be4"));

            migrationBuilder.DeleteData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("35a71ae7-49c1-4878-a1db-edcd2834f1cd"));

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("03d12e43-250b-49b7-bd08-b12135e47723"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"H\\u1ECDc sinh, sinh vi\\u00EAn\",\"image\":\"student.svg\"},{\"id\":2,\"content\":\"Ng\\u01B0\\u1EDDi \\u0111i l\\u00E0m\",\"image\":\"worker.svg\"}]");
        }
    }
}
