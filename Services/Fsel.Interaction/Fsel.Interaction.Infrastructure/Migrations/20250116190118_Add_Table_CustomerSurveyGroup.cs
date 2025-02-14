using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Fsel.Interaction.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_Table_CustomerSurveyGroup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CompetitionEventId",
                table: "SurveyQuestions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SurveyFormType",
                table: "SurveyQuestions",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "CustomerSurveys",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CustomerSurveyGroupId",
                table: "CustomerSurveys",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CustomerSurveyGroups",
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
                    Status = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SurveyFormType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CompetitionEventId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Coin = table.Column<double>(type: "float", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerSurveyGroups", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("03d12e43-250b-49b7-bd08-b12135e47723"),
                columns: new[] { "CompetitionEventId", "SurveyFormType" },
                values: new object[] { null, "QuestBoard" });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("06b1a0df-eaef-4be9-a9da-9c2633e3a12a"),
                columns: new[] { "CompetitionEventId", "SurveyFormType" },
                values: new object[] { null, "Default" });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("0f9df809-9ae9-4e8c-891d-ec645483f290"),
                columns: new[] { "CompetitionEventId", "SurveyFormType" },
                values: new object[] { null, "Default" });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("14787cbf-cc43-4453-a148-6d11a683f311"),
                columns: new[] { "CompetitionEventId", "SurveyFormType" },
                values: new object[] { null, "QuestBoard" });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("1fde61af-e1e0-4027-ad21-1176ad212119"),
                columns: new[] { "CompetitionEventId", "SurveyFormType" },
                values: new object[] { null, "Default" });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("20675350-89f2-47e3-8e64-b04f2da6290b"),
                columns: new[] { "CompetitionEventId", "SurveyFormType" },
                values: new object[] { null, "Default" });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("2be9a620-143d-41f6-815b-2038c21a7b23"),
                columns: new[] { "CompetitionEventId", "SurveyFormType" },
                values: new object[] { null, "Default" });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("35a71ae7-49c1-4878-a1db-edcd2834f1cd"),
                columns: new[] { "CompetitionEventId", "SurveyFormType" },
                values: new object[] { null, "QuestBoard" });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("492d8bb9-cdbe-42e7-aa16-35a1915c3621"),
                columns: new[] { "CompetitionEventId", "SurveyFormType" },
                values: new object[] { null, "Default" });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("52548bc5-3536-478c-978b-05f98815bf31"),
                columns: new[] { "CompetitionEventId", "SurveyFormType" },
                values: new object[] { null, "Default" });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("56628f4a-0384-426c-b849-81ab1671afa4"),
                columns: new[] { "CompetitionEventId", "SurveyFormType" },
                values: new object[] { null, "Default" });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("651414ad-cbcf-462a-84b9-daf83880b25a"),
                columns: new[] { "CompetitionEventId", "SurveyFormType" },
                values: new object[] { null, "Default" });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("713d8bb9-cdbe-42e7-aa16-35a1915c3532"),
                columns: new[] { "CompetitionEventId", "SurveyFormType" },
                values: new object[] { null, "Default" });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("721d8bb9-cdbe-42e7-aa16-35a1915c1123"),
                columns: new[] { "CompetitionEventId", "SurveyFormType" },
                values: new object[] { null, "Default" });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("73ab87b1-c0ae-40c6-ba37-1cdce350563a"),
                columns: new[] { "CompetitionEventId", "SurveyFormType" },
                values: new object[] { null, "Default" });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("763d8bb9-cdbe-42e7-aa16-35a1915c3512"),
                columns: new[] { "CompetitionEventId", "SurveyFormType" },
                values: new object[] { null, "Default" });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("83a5b867-c3a1-47b9-91e3-a6fd6086501e"),
                columns: new[] { "CompetitionEventId", "SurveyFormType" },
                values: new object[] { null, "Default" });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("9619b7d3-b2ab-4462-9044-b443ab79942a"),
                columns: new[] { "CompetitionEventId", "SurveyFormType" },
                values: new object[] { null, "Default" });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("98a97cf1-8562-4828-9cf2-6f5823bf09fe"),
                columns: new[] { "CompetitionEventId", "SurveyFormType" },
                values: new object[] { null, "Default" });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("a6f6b5dc-c72a-496c-859c-2fdc9d97f147"),
                columns: new[] { "CompetitionEventId", "SurveyFormType" },
                values: new object[] { null, "Default" });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("b223125a-a4e1-4e10-b4dd-cfcd747d74c5"),
                columns: new[] { "CompetitionEventId", "SurveyFormType" },
                values: new object[] { null, "QuestBoard" });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("bb50bb8b-4e72-4a5b-a058-cdbc7837f4dc"),
                columns: new[] { "CompetitionEventId", "SurveyFormType" },
                values: new object[] { null, "Default" });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("cedef58c-7d94-4efc-a030-0216b3034bef"),
                columns: new[] { "CompetitionEventId", "SurveyFormType" },
                values: new object[] { null, "Default" });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("ee0e74f5-83ae-44dd-a7d0-0f7b650884f8"),
                columns: new[] { "CompetitionEventId", "SurveyFormType" },
                values: new object[] { null, "Default" });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("ef128343-2e29-4735-82f7-9d8d2231dcc2"),
                columns: new[] { "CompetitionEventId", "SurveyFormType" },
                values: new object[] { null, "Default" });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("f05b02dc-6393-41dd-a479-cc9c05d1da93"),
                columns: new[] { "CompetitionEventId", "SurveyFormType" },
                values: new object[] { null, "Default" });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("f563da40-d609-4922-90b9-44e4290edfef"),
                columns: new[] { "CompetitionEventId", "SurveyFormType" },
                values: new object[] { null, "QuestBoard" });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("f959d3d1-cab3-4fc6-8341-cb44bcbd3e30"),
                columns: new[] { "CompetitionEventId", "SurveyFormType" },
                values: new object[] { null, "Default" });

            migrationBuilder.InsertData(
                table: "SurveyQuestions",
                columns: new[] { "Id", "AnswerStr", "CompetitionEventId", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "Description", "DisplayLevel", "DisplayOrder", "Icon", "IsDeleted", "IsPilot", "Question", "SurveyFormType", "Type", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[,]
                {
                    { new Guid("238cae89-e1d8-408a-bb3c-1a0b27cd0edb"), "[{\"id\":1,\"content\":\"C\\u1EA3i thi\\u1EC7n \\u0111i\\u1EC3m s\\u1ED1 \\u1EDF tr\\u01B0\\u1EDDng\",\"image\":null},{\"id\":2,\"content\":\"Thi ch\\u1EE9ng ch\\u1EC9\",\"image\":null},{\"id\":3,\"content\":\"S\\u1EDF th\\u00EDch\",\"image\":null},{\"id\":4,\"content\":\"L\\u00FD do kh\\u00E1c\",\"image\":null}]", new Guid("ae2aa832-8436-4ca8-87c4-a3b563b7b5f3"), new DateTime(2025, 1, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, 1, 1f, null, false, false, "Mục tiêu của bạn khi học Tiếng Anh là gì?", "Event", "MultipleChoiceVertical", null, null, null },
                    { new Guid("68c17fa9-1d90-4eb2-8412-ea5062ad26f0"), "[{\"id\":1,\"content\":\"1\",\"image\":null},{\"id\":2,\"content\":\"2\",\"image\":null},{\"id\":3,\"content\":\"3\",\"image\":null},{\"id\":4,\"content\":\"4\",\"image\":null},{\"id\":5,\"content\":\"5\",\"image\":null}]", new Guid("ae2aa832-8436-4ca8-87c4-a3b563b7b5f3"), new DateTime(2025, 1, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, 1, 2f, null, false, false, "Bạn đánh giá mức độ quan trọng của việc học tiếng Anh như thế nào (thang điểm 5 từ thấp đến cao)?", "Event", "MultipleChoiceVertical", null, null, null },
                    { new Guid("73a7a0c8-1e07-4351-b3f4-3e843623bbaf"), "[{\"id\":1,\"content\":\"Trung t\\u00E2m ti\\u1EBFng Anh\",\"image\":null},{\"id\":2,\"content\":\"H\\u1ECDc online\",\"image\":null},{\"id\":3,\"content\":\"H\\u1ECDc gia s\\u01B0\",\"image\":null},{\"id\":4,\"content\":\"H\\u1ECDc th\\u00EAm v\\u1EDBi gi\\u00E1o vi\\u00EAn\",\"image\":null},{\"id\":5,\"content\":\"Kh\\u00F4ng h\\u1ECDc th\\u00EAm ti\\u1EBFng Anh\",\"image\":null}]", new Guid("ae2aa832-8436-4ca8-87c4-a3b563b7b5f3"), new DateTime(2025, 1, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, 2, 1f, null, false, false, "Bạn học thêm Tiếng Anh ở đâu?", "Event", "MultipleChoiceVertical", null, null, null },
                    { new Guid("c6bda257-0757-4290-86c1-794d0ba6ae3b"), "[{\"id\":1,\"content\":\"Giao ti\\u1EBFp ti\\u1EBFng Anh r\\u1EA5t pro trong m\\u1EAFt b\\u1EA1n b\\u00E8\",\"image\":null},{\"id\":2,\"content\":\"\\u0110i\\u1EC3m cao \\u0111\\u01B0\\u1EE3c c\\u00F4 gi\\u00E1o khen ng\\u1EE3i tr\\u01B0\\u1EDBc l\\u1EDBp\",\"image\":null},{\"id\":3,\"content\":\"Th\\u00EDch v\\u00EC ti\\u1EBFng Anh hay ho\",\"image\":null},{\"id\":4,\"content\":\"Kh\\u00F4ng th\\u00EDch g\\u00EC c\\u1EA3\",\"image\":null},{\"id\":5,\"content\":\"Gh\\u00E9t ti\\u1EBFng Anh\",\"image\":null}]", new Guid("ae2aa832-8436-4ca8-87c4-a3b563b7b5f3"), new DateTime(2025, 1, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, 2, 2f, null, false, false, "Điều bạn thích nhất ở việc học tiếng Anh là gì?", "Event", "MultipleChoiceVertical", null, null, null }
                });

            migrationBuilder.InsertData(
                table: "SurveyQuestionTranslations",
                columns: new[] { "Id", "AnswerStr", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "Description", "IsDeleted", "Language", "Question", "SurveyQuestionId", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[,]
                {
                    { new Guid("0bced45a-cc34-47d4-a1e5-97735f52791b"), "[{\"id\":1,\"content\":\"Centres de langue anglaise\",\"image\":null},{\"id\":2,\"content\":\"Apprentissage en ligne\",\"image\":null},{\"id\":3,\"content\":\"Cours particuliers\",\"image\":null},{\"id\":4,\"content\":\"Cours suppl\\u00E9mentaires avec des enseignants\",\"image\":null},{\"id\":5,\"content\":\"Je ne prends pas de cours suppl\\u00E9mentaires d\\u0027anglais\",\"image\":null}]", new DateTime(2025, 1, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "fr-FR", "Où prenez-vous des cours supplémentaires d'anglais ?", new Guid("73a7a0c8-1e07-4351-b3f4-3e843623bbaf"), null, null, null },
                    { new Guid("1e81a3d3-edcb-4401-8a59-870fdbf037e5"), "[{\"id\":1,\"content\":\"Being seen as a pro at English by friends\",\"image\":null},{\"id\":2,\"content\":\"High grades and being praised by the teacher in class\",\"image\":null},{\"id\":3,\"content\":\"Enjoying English because it is interesting\",\"image\":null},{\"id\":4,\"content\":\"Do not like anything about it\",\"image\":null},{\"id\":5,\"content\":\"Dislike English entirely\",\"image\":null}]", new DateTime(2025, 1, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "en-US", "What do you like most about learning English?", new Guid("c6bda257-0757-4290-86c1-794d0ba6ae3b"), null, null, null },
                    { new Guid("3353fb66-5d41-4334-9542-510e327359c7"), "[{\"id\":1,\"content\":\"English language centers\",\"image\":null},{\"id\":2,\"content\":\"Online learning\",\"image\":null},{\"id\":3,\"content\":\"Private tutoring\",\"image\":null},{\"id\":4,\"content\":\"Extra classes with teachers\",\"image\":null},{\"id\":5,\"content\":\"Do not take additional English lessons\",\"image\":null}]", new DateTime(2025, 1, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "en-US", "Where do you take additional English lessons?", new Guid("73a7a0c8-1e07-4351-b3f4-3e843623bbaf"), null, null, null },
                    { new Guid("41058318-f3c0-4285-81c9-ac847a1d5364"), "[{\"id\":1,\"content\":\"Improving school grades\",\"image\":null},{\"id\":2,\"content\":\"Taking certification exams\",\"image\":null},{\"id\":3,\"content\":\"Personal interest\",\"image\":null},{\"id\":4,\"content\":\"Other reasons\",\"image\":null}]", new DateTime(2025, 1, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "en-US", "What is your goal in learning English?", new Guid("238cae89-e1d8-408a-bb3c-1a0b27cd0edb"), null, null, null },
                    { new Guid("54d7630b-e27a-4e89-a77b-23384e15dc9b"), "[{\"id\":1,\"content\":\"1\",\"image\":null},{\"id\":2,\"content\":\"2\",\"image\":null},{\"id\":3,\"content\":\"3\",\"image\":null},{\"id\":4,\"content\":\"4\",\"image\":null},{\"id\":5,\"content\":\"5\",\"image\":null}]", new DateTime(2025, 1, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "vi-VN", "Bạn đánh giá mức độ quan trọng của việc học tiếng Anh như thế nào (thang điểm 5 từ thấp đến cao)?", new Guid("68c17fa9-1d90-4eb2-8412-ea5062ad26f0"), null, null, null },
                    { new Guid("70c9d600-a6a2-4d56-a740-3d0151efab1a"), "[{\"id\":1,\"content\":\"Giao ti\\u1EBFp ti\\u1EBFng Anh r\\u1EA5t pro trong m\\u1EAFt b\\u1EA1n b\\u00E8\",\"image\":null},{\"id\":2,\"content\":\"\\u0110i\\u1EC3m cao \\u0111\\u01B0\\u1EE3c c\\u00F4 gi\\u00E1o khen ng\\u1EE3i tr\\u01B0\\u1EDBc l\\u1EDBp\",\"image\":null},{\"id\":3,\"content\":\"Th\\u00EDch v\\u00EC ti\\u1EBFng Anh hay ho\",\"image\":null},{\"id\":4,\"content\":\"Kh\\u00F4ng th\\u00EDch g\\u00EC c\\u1EA3\",\"image\":null},{\"id\":5,\"content\":\"Gh\\u00E9t ti\\u1EBFng Anh\",\"image\":null}]", new DateTime(2025, 1, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "vi-VN", "Điều bạn thích nhất ở việc học tiếng Anh là gì?", new Guid("c6bda257-0757-4290-86c1-794d0ba6ae3b"), null, null, null },
                    { new Guid("819f8198-3b77-4243-9dfc-4f5b2ae052a7"), "[{\"id\":1,\"content\":\"1\",\"image\":null},{\"id\":2,\"content\":\"2\",\"image\":null},{\"id\":3,\"content\":\"3\",\"image\":null},{\"id\":4,\"content\":\"4\",\"image\":null},{\"id\":5,\"content\":\"5\",\"image\":null}]", new DateTime(2025, 1, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "en-US", "How would you rate the importance of learning English (on a scale of 1 to 5, from least to most important)?", new Guid("68c17fa9-1d90-4eb2-8412-ea5062ad26f0"), null, null, null },
                    { new Guid("8bb2b7f3-8254-4e83-b13f-f43cdb8d3c18"), "[{\"id\":1,\"content\":\"Am\\u00E9liorer vos notes \\u00E0 l\\u0027\\u00E9cole\",\"image\":null},{\"id\":2,\"content\":\"Passer une certification\",\"image\":null},{\"id\":3,\"content\":\"Int\\u00E9r\\u00EAt personnel\",\"image\":null},{\"id\":4,\"content\":\"Autres raisons\",\"image\":null}]", new DateTime(2025, 1, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "fr-FR", "Quels sont vos objectifs pour apprendre l'anglais ?", new Guid("238cae89-e1d8-408a-bb3c-1a0b27cd0edb"), null, null, null },
                    { new Guid("90267a54-b716-43be-a54d-bae0cec65729"), "[{\"id\":1,\"content\":\"1\",\"image\":null},{\"id\":2,\"content\":\"2\",\"image\":null},{\"id\":3,\"content\":\"3\",\"image\":null},{\"id\":4,\"content\":\"4\",\"image\":null},{\"id\":5,\"content\":\"5\",\"image\":null}]", new DateTime(2025, 1, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "fr-FR", "Comment évaluez-vous l'importance de l'apprentissage de l'anglais ? (échelle de 1 à 5, de faible à élevé)?", new Guid("68c17fa9-1d90-4eb2-8412-ea5062ad26f0"), null, null, null },
                    { new Guid("97064707-b3d8-4fb5-bf4a-12bb6eb3776e"), "[{\"id\":1,\"content\":\"\\u00CAtre consid\\u00E9r\\u00E9 comme un expert en anglais par vos amis\",\"image\":null},{\"id\":2,\"content\":\"Obtenir de bonnes notes et \\u00EAtre f\\u00E9licit\\u00E9 par le professeur en classe\",\"image\":null},{\"id\":3,\"content\":\"Aimer l\\u0027anglais parce que c\\u0027est int\\u00E9ressant\",\"image\":null},{\"id\":4,\"content\":\"Ne rien aimer du tout\",\"image\":null},{\"id\":5,\"content\":\"D\\u00E9tester l\\u0027anglais compl\\u00E8tement\",\"image\":null}]", new DateTime(2025, 1, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "fr-FR", "Qu'aimez-vous le plus dans l'apprentissage de l'anglais ?", new Guid("c6bda257-0757-4290-86c1-794d0ba6ae3b"), null, null, null },
                    { new Guid("b3e92765-8b8d-4bec-a5d4-f9a9e213b53f"), "[{\"id\":1,\"content\":\"C\\u1EA3i thi\\u1EC7n \\u0111i\\u1EC3m s\\u1ED1 \\u1EDF tr\\u01B0\\u1EDDng\",\"image\":null},{\"id\":2,\"content\":\"Thi ch\\u1EE9ng ch\\u1EC9\",\"image\":null},{\"id\":3,\"content\":\"S\\u1EDF th\\u00EDch\",\"image\":null},{\"id\":4,\"content\":\"L\\u00FD do kh\\u00E1c\",\"image\":null}]", new DateTime(2025, 1, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "vi-VN", "Mục tiêu của bạn khi học Tiếng Anh là gì?", new Guid("238cae89-e1d8-408a-bb3c-1a0b27cd0edb"), null, null, null },
                    { new Guid("d4d2fae7-ae44-4652-883c-68c98a2972de"), "[{\"id\":1,\"content\":\"Trung t\\u00E2m ti\\u1EBFng Anh\",\"image\":null},{\"id\":2,\"content\":\"H\\u1ECDc online\",\"image\":null},{\"id\":3,\"content\":\"H\\u1ECDc gia s\\u01B0\",\"image\":null},{\"id\":4,\"content\":\"H\\u1ECDc th\\u00EAm v\\u1EDBi gi\\u00E1o vi\\u00EAn\",\"image\":null},{\"id\":5,\"content\":\"Kh\\u00F4ng h\\u1ECDc th\\u00EAm ti\\u1EBFng Anh\",\"image\":null}]", new DateTime(2025, 1, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "vi-VN", "Bạn học thêm Tiếng Anh ở đâu?", new Guid("73a7a0c8-1e07-4351-b3f4-3e843623bbaf"), null, null, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerSurveys_CustomerSurveyGroupId",
                table: "CustomerSurveys",
                column: "CustomerSurveyGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerSurveys_CustomerSurveyGroups_CustomerSurveyGroupId",
                table: "CustomerSurveys",
                column: "CustomerSurveyGroupId",
                principalTable: "CustomerSurveyGroups",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomerSurveys_CustomerSurveyGroups_CustomerSurveyGroupId",
                table: "CustomerSurveys");

            migrationBuilder.DropTable(
                name: "CustomerSurveyGroups");

            migrationBuilder.DropIndex(
                name: "IX_CustomerSurveys_CustomerSurveyGroupId",
                table: "CustomerSurveys");

            migrationBuilder.DeleteData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("0bced45a-cc34-47d4-a1e5-97735f52791b"));

            migrationBuilder.DeleteData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("1e81a3d3-edcb-4401-8a59-870fdbf037e5"));

            migrationBuilder.DeleteData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("3353fb66-5d41-4334-9542-510e327359c7"));

            migrationBuilder.DeleteData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("41058318-f3c0-4285-81c9-ac847a1d5364"));

            migrationBuilder.DeleteData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("54d7630b-e27a-4e89-a77b-23384e15dc9b"));

            migrationBuilder.DeleteData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("70c9d600-a6a2-4d56-a740-3d0151efab1a"));

            migrationBuilder.DeleteData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("819f8198-3b77-4243-9dfc-4f5b2ae052a7"));

            migrationBuilder.DeleteData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("8bb2b7f3-8254-4e83-b13f-f43cdb8d3c18"));

            migrationBuilder.DeleteData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("90267a54-b716-43be-a54d-bae0cec65729"));

            migrationBuilder.DeleteData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("97064707-b3d8-4fb5-bf4a-12bb6eb3776e"));

            migrationBuilder.DeleteData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("b3e92765-8b8d-4bec-a5d4-f9a9e213b53f"));

            migrationBuilder.DeleteData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("d4d2fae7-ae44-4652-883c-68c98a2972de"));

            migrationBuilder.DeleteData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("238cae89-e1d8-408a-bb3c-1a0b27cd0edb"));

            migrationBuilder.DeleteData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("68c17fa9-1d90-4eb2-8412-ea5062ad26f0"));

            migrationBuilder.DeleteData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("73a7a0c8-1e07-4351-b3f4-3e843623bbaf"));

            migrationBuilder.DeleteData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("c6bda257-0757-4290-86c1-794d0ba6ae3b"));

            migrationBuilder.DropColumn(
                name: "CompetitionEventId",
                table: "SurveyQuestions");

            migrationBuilder.DropColumn(
                name: "SurveyFormType",
                table: "SurveyQuestions");

            migrationBuilder.DropColumn(
                name: "CustomerSurveyGroupId",
                table: "CustomerSurveys");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "CustomerSurveys",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");
        }
    }
}
