using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Fsel.Interaction.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class add_survey_EVHaiDuong : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "SurveyQuestions",
                columns: new[] { "Id", "AnswerStr", "CompetitionEventId", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "Description", "DisplayLevel", "DisplayOrder", "Icon", "IsDeleted", "IsPilot", "Question", "SurveyFormType", "Type", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[,]
                {
                    { new Guid("0aafccb7-e700-4b30-866f-0b667217fa47"), "[{\"id\":1,\"content\":\"1\",\"image\":null},{\"id\":2,\"content\":\"2\",\"image\":null},{\"id\":3,\"content\":\"3\",\"image\":null},{\"id\":4,\"content\":\"4\",\"image\":null},{\"id\":5,\"content\":\"5\",\"image\":null}]", new Guid("b0a2d65c-997c-4b10-afff-d024ae736c27"), new DateTime(2025, 1, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, 1, 2f, null, false, false, "Bạn đánh giá mức độ quan trọng của việc học tiếng Anh như thế nào (thang điểm 5 từ thấp đến cao)?", "Event", "MultipleChoiceVertical", null, null, null },
                    { new Guid("482c87c7-b555-4de2-a418-f990960e0ee6"), "[{\"id\":1,\"content\":\"C\\u1EA3i thi\\u1EC7n \\u0111i\\u1EC3m s\\u1ED1 \\u1EDF tr\\u01B0\\u1EDDng\",\"image\":null},{\"id\":2,\"content\":\"Thi ch\\u1EE9ng ch\\u1EC9\",\"image\":null},{\"id\":3,\"content\":\"S\\u1EDF th\\u00EDch\",\"image\":null},{\"id\":4,\"content\":\"L\\u00FD do kh\\u00E1c\",\"image\":null}]", new Guid("b0a2d65c-997c-4b10-afff-d024ae736c27"), new DateTime(2025, 1, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, 1, 1f, null, false, false, "Mục tiêu của bạn khi học Tiếng Anh là gì?", "Event", "MultipleChoiceVertical", null, null, null },
                    { new Guid("c88e157b-1496-4aae-b81f-dd0718524e38"), "[{\"id\":1,\"content\":\"Giao ti\\u1EBFp ti\\u1EBFng Anh r\\u1EA5t pro trong m\\u1EAFt b\\u1EA1n b\\u00E8\",\"image\":null},{\"id\":2,\"content\":\"\\u0110i\\u1EC3m cao \\u0111\\u01B0\\u1EE3c c\\u00F4 gi\\u00E1o khen ng\\u1EE3i tr\\u01B0\\u1EDBc l\\u1EDBp\",\"image\":null},{\"id\":3,\"content\":\"Th\\u00EDch v\\u00EC ti\\u1EBFng Anh hay ho\",\"image\":null},{\"id\":4,\"content\":\"Kh\\u00F4ng th\\u00EDch g\\u00EC c\\u1EA3\",\"image\":null},{\"id\":5,\"content\":\"Gh\\u00E9t ti\\u1EBFng Anh\",\"image\":null}]", new Guid("b0a2d65c-997c-4b10-afff-d024ae736c27"), new DateTime(2025, 1, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, 2, 2f, null, false, false, "Điều bạn thích nhất ở việc học tiếng Anh là gì?", "Event", "MultipleChoiceVertical", null, null, null },
                    { new Guid("da618b05-f9b8-4bfd-8bae-7a5adb668d16"), "[{\"id\":1,\"content\":\"Trung t\\u00E2m ti\\u1EBFng Anh\",\"image\":null},{\"id\":2,\"content\":\"H\\u1ECDc online\",\"image\":null},{\"id\":3,\"content\":\"H\\u1ECDc gia s\\u01B0\",\"image\":null},{\"id\":4,\"content\":\"H\\u1ECDc th\\u00EAm v\\u1EDBi gi\\u00E1o vi\\u00EAn\",\"image\":null},{\"id\":5,\"content\":\"Kh\\u00F4ng h\\u1ECDc th\\u00EAm ti\\u1EBFng Anh\",\"image\":null}]", new Guid("b0a2d65c-997c-4b10-afff-d024ae736c27"), new DateTime(2025, 1, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, 2, 1f, null, false, false, "Bạn học thêm Tiếng Anh ở đâu?", "Event", "MultipleChoiceVertical", null, null, null }
                });

            migrationBuilder.InsertData(
                table: "SurveyQuestionTranslations",
                columns: new[] { "Id", "AnswerStr", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "Description", "IsDeleted", "Language", "Question", "SurveyQuestionId", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[,]
                {
                    { new Guid("10f84b12-becb-4981-b2f9-6a24d2b2a7b7"), "[{\"id\":1,\"content\":\"English language centers\",\"image\":null},{\"id\":2,\"content\":\"Online learning\",\"image\":null},{\"id\":3,\"content\":\"Private tutoring\",\"image\":null},{\"id\":4,\"content\":\"Extra classes with teachers\",\"image\":null},{\"id\":5,\"content\":\"Do not take additional English lessons\",\"image\":null}]", new DateTime(2025, 1, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "en-US", "Where do you take additional English lessons?", new Guid("da618b05-f9b8-4bfd-8bae-7a5adb668d16"), null, null, null },
                    { new Guid("30adf928-8e15-4efd-9c35-e31798381a26"), "[{\"id\":1,\"content\":\"Am\\u00E9liorer vos notes \\u00E0 l\\u0027\\u00E9cole\",\"image\":null},{\"id\":2,\"content\":\"Passer une certification\",\"image\":null},{\"id\":3,\"content\":\"Int\\u00E9r\\u00EAt personnel\",\"image\":null},{\"id\":4,\"content\":\"Autres raisons\",\"image\":null}]", new DateTime(2025, 1, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "fr-FR", "Quels sont vos objectifs pour apprendre l'anglais ?", new Guid("482c87c7-b555-4de2-a418-f990960e0ee6"), null, null, null },
                    { new Guid("37edc69a-d243-4e19-a002-6e090bfb6871"), "[{\"id\":1,\"content\":\"Being seen as a pro at English by friends\",\"image\":null},{\"id\":2,\"content\":\"High grades and being praised by the teacher in class\",\"image\":null},{\"id\":3,\"content\":\"Enjoying English because it is interesting\",\"image\":null},{\"id\":4,\"content\":\"Do not like anything about it\",\"image\":null},{\"id\":5,\"content\":\"Dislike English entirely\",\"image\":null}]", new DateTime(2025, 1, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "en-US", "What do you like most about learning English?", new Guid("c88e157b-1496-4aae-b81f-dd0718524e38"), null, null, null },
                    { new Guid("4fabc8f2-fcf1-4756-9e2e-64877d8a00da"), "[{\"id\":1,\"content\":\"C\\u1EA3i thi\\u1EC7n \\u0111i\\u1EC3m s\\u1ED1 \\u1EDF tr\\u01B0\\u1EDDng\",\"image\":null},{\"id\":2,\"content\":\"Thi ch\\u1EE9ng ch\\u1EC9\",\"image\":null},{\"id\":3,\"content\":\"S\\u1EDF th\\u00EDch\",\"image\":null},{\"id\":4,\"content\":\"L\\u00FD do kh\\u00E1c\",\"image\":null}]", new DateTime(2025, 1, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "vi-VN", "Mục tiêu của bạn khi học Tiếng Anh là gì?", new Guid("482c87c7-b555-4de2-a418-f990960e0ee6"), null, null, null },
                    { new Guid("66f876c3-716a-4b81-b790-e13e37fceedc"), "[{\"id\":1,\"content\":\"Centres de langue anglaise\",\"image\":null},{\"id\":2,\"content\":\"Apprentissage en ligne\",\"image\":null},{\"id\":3,\"content\":\"Cours particuliers\",\"image\":null},{\"id\":4,\"content\":\"Cours suppl\\u00E9mentaires avec des enseignants\",\"image\":null},{\"id\":5,\"content\":\"Je ne prends pas de cours suppl\\u00E9mentaires d\\u0027anglais\",\"image\":null}]", new DateTime(2025, 1, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "fr-FR", "Où prenez-vous des cours supplémentaires d'anglais ?", new Guid("da618b05-f9b8-4bfd-8bae-7a5adb668d16"), null, null, null },
                    { new Guid("6afb9363-cf97-4dd2-a0e5-fd29baa6aafd"), "[{\"id\":1,\"content\":\"1\",\"image\":null},{\"id\":2,\"content\":\"2\",\"image\":null},{\"id\":3,\"content\":\"3\",\"image\":null},{\"id\":4,\"content\":\"4\",\"image\":null},{\"id\":5,\"content\":\"5\",\"image\":null}]", new DateTime(2025, 1, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "fr-FR", "Comment évaluez-vous l'importance de l'apprentissage de l'anglais ? (échelle de 1 à 5, de faible à élevé)?", new Guid("0aafccb7-e700-4b30-866f-0b667217fa47"), null, null, null },
                    { new Guid("75f618e3-7a5e-4713-9e92-8dadbdfb3035"), "[{\"id\":1,\"content\":\"Improving school grades\",\"image\":null},{\"id\":2,\"content\":\"Taking certification exams\",\"image\":null},{\"id\":3,\"content\":\"Personal interest\",\"image\":null},{\"id\":4,\"content\":\"Other reasons\",\"image\":null}]", new DateTime(2025, 1, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "en-US", "What is your goal in learning English?", new Guid("482c87c7-b555-4de2-a418-f990960e0ee6"), null, null, null },
                    { new Guid("80f7d9f6-39b2-4dff-a71e-92cf810c7b73"), "[{\"id\":1,\"content\":\"Trung t\\u00E2m ti\\u1EBFng Anh\",\"image\":null},{\"id\":2,\"content\":\"H\\u1ECDc online\",\"image\":null},{\"id\":3,\"content\":\"H\\u1ECDc gia s\\u01B0\",\"image\":null},{\"id\":4,\"content\":\"H\\u1ECDc th\\u00EAm v\\u1EDBi gi\\u00E1o vi\\u00EAn\",\"image\":null},{\"id\":5,\"content\":\"Kh\\u00F4ng h\\u1ECDc th\\u00EAm ti\\u1EBFng Anh\",\"image\":null}]", new DateTime(2025, 1, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "vi-VN", "Bạn học thêm Tiếng Anh ở đâu?", new Guid("da618b05-f9b8-4bfd-8bae-7a5adb668d16"), null, null, null },
                    { new Guid("8df0dfba-846e-4510-99f1-0d76ec75f78d"), "[{\"id\":1,\"content\":\"1\",\"image\":null},{\"id\":2,\"content\":\"2\",\"image\":null},{\"id\":3,\"content\":\"3\",\"image\":null},{\"id\":4,\"content\":\"4\",\"image\":null},{\"id\":5,\"content\":\"5\",\"image\":null}]", new DateTime(2025, 1, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "en-US", "How would you rate the importance of learning English (on a scale of 1 to 5, from least to most important)?", new Guid("0aafccb7-e700-4b30-866f-0b667217fa47"), null, null, null },
                    { new Guid("db2b2556-74cf-423e-8620-2ac1246baf1e"), "[{\"id\":1,\"content\":\"Giao ti\\u1EBFp ti\\u1EBFng Anh r\\u1EA5t pro trong m\\u1EAFt b\\u1EA1n b\\u00E8\",\"image\":null},{\"id\":2,\"content\":\"\\u0110i\\u1EC3m cao \\u0111\\u01B0\\u1EE3c c\\u00F4 gi\\u00E1o khen ng\\u1EE3i tr\\u01B0\\u1EDBc l\\u1EDBp\",\"image\":null},{\"id\":3,\"content\":\"Th\\u00EDch v\\u00EC ti\\u1EBFng Anh hay ho\",\"image\":null},{\"id\":4,\"content\":\"Kh\\u00F4ng th\\u00EDch g\\u00EC c\\u1EA3\",\"image\":null},{\"id\":5,\"content\":\"Gh\\u00E9t ti\\u1EBFng Anh\",\"image\":null}]", new DateTime(2025, 1, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "vi-VN", "Điều bạn thích nhất ở việc học tiếng Anh là gì?", new Guid("c88e157b-1496-4aae-b81f-dd0718524e38"), null, null, null },
                    { new Guid("e7a3a48e-e048-4514-a5a4-f1229dcfdfd0"), "[{\"id\":1,\"content\":\"1\",\"image\":null},{\"id\":2,\"content\":\"2\",\"image\":null},{\"id\":3,\"content\":\"3\",\"image\":null},{\"id\":4,\"content\":\"4\",\"image\":null},{\"id\":5,\"content\":\"5\",\"image\":null}]", new DateTime(2025, 1, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "vi-VN", "Bạn đánh giá mức độ quan trọng của việc học tiếng Anh như thế nào (thang điểm 5 từ thấp đến cao)?", new Guid("0aafccb7-e700-4b30-866f-0b667217fa47"), null, null, null },
                    { new Guid("fb59b7d2-ed36-4c5e-9fc5-4f176c9a5873"), "[{\"id\":1,\"content\":\"\\u00CAtre consid\\u00E9r\\u00E9 comme un expert en anglais par vos amis\",\"image\":null},{\"id\":2,\"content\":\"Obtenir de bonnes notes et \\u00EAtre f\\u00E9licit\\u00E9 par le professeur en classe\",\"image\":null},{\"id\":3,\"content\":\"Aimer l\\u0027anglais parce que c\\u0027est int\\u00E9ressant\",\"image\":null},{\"id\":4,\"content\":\"Ne rien aimer du tout\",\"image\":null},{\"id\":5,\"content\":\"D\\u00E9tester l\\u0027anglais compl\\u00E8tement\",\"image\":null}]", new DateTime(2025, 1, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "fr-FR", "Qu'aimez-vous le plus dans l'apprentissage de l'anglais ?", new Guid("c88e157b-1496-4aae-b81f-dd0718524e38"), null, null, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("10f84b12-becb-4981-b2f9-6a24d2b2a7b7"));

            migrationBuilder.DeleteData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("30adf928-8e15-4efd-9c35-e31798381a26"));

            migrationBuilder.DeleteData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("37edc69a-d243-4e19-a002-6e090bfb6871"));

            migrationBuilder.DeleteData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("4fabc8f2-fcf1-4756-9e2e-64877d8a00da"));

            migrationBuilder.DeleteData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("66f876c3-716a-4b81-b790-e13e37fceedc"));

            migrationBuilder.DeleteData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("6afb9363-cf97-4dd2-a0e5-fd29baa6aafd"));

            migrationBuilder.DeleteData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("75f618e3-7a5e-4713-9e92-8dadbdfb3035"));

            migrationBuilder.DeleteData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("80f7d9f6-39b2-4dff-a71e-92cf810c7b73"));

            migrationBuilder.DeleteData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("8df0dfba-846e-4510-99f1-0d76ec75f78d"));

            migrationBuilder.DeleteData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("db2b2556-74cf-423e-8620-2ac1246baf1e"));

            migrationBuilder.DeleteData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("e7a3a48e-e048-4514-a5a4-f1229dcfdfd0"));

            migrationBuilder.DeleteData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("fb59b7d2-ed36-4c5e-9fc5-4f176c9a5873"));

            migrationBuilder.DeleteData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("0aafccb7-e700-4b30-866f-0b667217fa47"));

            migrationBuilder.DeleteData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("482c87c7-b555-4de2-a418-f990960e0ee6"));

            migrationBuilder.DeleteData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("c88e157b-1496-4aae-b81f-dd0718524e38"));

            migrationBuilder.DeleteData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("da618b05-f9b8-4bfd-8bae-7a5adb668d16"));
        }
    }
}
