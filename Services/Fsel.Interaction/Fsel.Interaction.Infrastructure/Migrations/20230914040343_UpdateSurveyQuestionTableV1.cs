using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Fsel.Interaction.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSurveyQuestionTableV1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("5317ae1a-46db-4f25-9625-f9c28bd3c688"));

            migrationBuilder.AlterColumn<float>(
                name: "DisplayOrder",
                table: "SurveyQuestions",
                type: "real",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "DisplayLevel",
                table: "SurveyQuestions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("06b1a0df-eaef-4be9-a9da-9c2633e3a12a"),
                columns: new[] { "AnswerStr", "DisplayLevel", "DisplayOrder" },
                values: new object[] { "[{\"id\":1,\"content\":\"M\\u00E1y t\\u00EDnh x\\u00E1ch tay\",\"referenceQuestionId\":\"83a5b867-c3a1-47b9-91e3-a6fd6086501e\"},{\"id\":2,\"content\":\"M\\u00E1y t\\u00EDnh \\u0111\\u1EC3 b\\u00E0n\",\"referenceQuestionId\":null},{\"id\":3,\"content\":\"C\\u1EA3 2\",\"referenceQuestionId\":\"83a5b867-c3a1-47b9-91e3-a6fd6086501e\"},{\"id\":4,\"content\":\"No\",\"referenceQuestionId\":null}]", 2, 5f });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("0f9df809-9ae9-4e8c-891d-ec645483f290"),
                columns: new[] { "AnswerStr", "DisplayLevel", "DisplayOrder", "Question" },
                values: new object[] { "[{\"id\":1,\"content\":\"\\u003C11\",\"referenceQuestionId\":[\"1fde61af-e1e0-4027-ad21-1176ad212119\",\"5317ae1a-46db-4f25-9625-f9c28bd3c688\"]},{\"id\":2,\"content\":\"11\",\"referenceQuestionId\":[\"73ab87b1-c0ae-40c6-ba37-1cdce350563a\",\"06b1a0df-eaef-4be9-a9da-9c2633e3a12a\",\"56628f4a-0384-426c-b849-81ab1671afa4\",\"f05b02dc-6393-41dd-a479-cc9c05d1da93\",\"bb50bb8b-4e72-4a5b-a058-cdbc7837f4dc\",\"ef128343-2e29-4735-82f7-9d8d2231dcc2\",\"20675350-89f2-47e3-8e64-b04f2da6290b\",\"98a97cf1-8562-4828-9cf2-6f5823bf09fe\",\"1fde61af-e1e0-4027-ad21-1176ad212119\"]},{\"id\":3,\"content\":\"12\",\"referenceQuestionId\":[\"73ab87b1-c0ae-40c6-ba37-1cdce350563a\",\"06b1a0df-eaef-4be9-a9da-9c2633e3a12a\",\"56628f4a-0384-426c-b849-81ab1671afa4\",\"f05b02dc-6393-41dd-a479-cc9c05d1da93\",\"bb50bb8b-4e72-4a5b-a058-cdbc7837f4dc\",\"ef128343-2e29-4735-82f7-9d8d2231dcc2\",\"20675350-89f2-47e3-8e64-b04f2da6290b\",\"98a97cf1-8562-4828-9cf2-6f5823bf09fe\",\"1fde61af-e1e0-4027-ad21-1176ad212119\"]},{\"id\":4,\"content\":\"13\",\"referenceQuestionId\":[\"73ab87b1-c0ae-40c6-ba37-1cdce350563a\",\"06b1a0df-eaef-4be9-a9da-9c2633e3a12a\",\"56628f4a-0384-426c-b849-81ab1671afa4\",\"f05b02dc-6393-41dd-a479-cc9c05d1da93\",\"bb50bb8b-4e72-4a5b-a058-cdbc7837f4dc\",\"ef128343-2e29-4735-82f7-9d8d2231dcc2\",\"20675350-89f2-47e3-8e64-b04f2da6290b\",\"98a97cf1-8562-4828-9cf2-6f5823bf09fe\",\"1fde61af-e1e0-4027-ad21-1176ad212119\"]},{\"id\":5,\"content\":\"14\",\"referenceQuestionId\":[\"73ab87b1-c0ae-40c6-ba37-1cdce350563a\",\"06b1a0df-eaef-4be9-a9da-9c2633e3a12a\",\"56628f4a-0384-426c-b849-81ab1671afa4\",\"f05b02dc-6393-41dd-a479-cc9c05d1da93\",\"bb50bb8b-4e72-4a5b-a058-cdbc7837f4dc\",\"ef128343-2e29-4735-82f7-9d8d2231dcc2\",\"20675350-89f2-47e3-8e64-b04f2da6290b\",\"98a97cf1-8562-4828-9cf2-6f5823bf09fe\",\"1fde61af-e1e0-4027-ad21-1176ad212119\"]},{\"id\":6,\"content\":\"15\",\"referenceQuestionId\":[\"73ab87b1-c0ae-40c6-ba37-1cdce350563a\",\"06b1a0df-eaef-4be9-a9da-9c2633e3a12a\",\"56628f4a-0384-426c-b849-81ab1671afa4\",\"f05b02dc-6393-41dd-a479-cc9c05d1da93\",\"bb50bb8b-4e72-4a5b-a058-cdbc7837f4dc\",\"ef128343-2e29-4735-82f7-9d8d2231dcc2\",\"20675350-89f2-47e3-8e64-b04f2da6290b\",\"98a97cf1-8562-4828-9cf2-6f5823bf09fe\",\"1fde61af-e1e0-4027-ad21-1176ad212119\"]},{\"id\":7,\"content\":\"16\",\"referenceQuestionId\":[\"fcd14833-b79f-4cf4-9ee8-65b041a7daf1\",\"06b1a0df-eaef-4be9-a9da-9c2633e3a12a\",\"56628f4a-0384-426c-b849-81ab1671afa4\",\"f05b02dc-6393-41dd-a479-cc9c05d1da93\",\"bb50bb8b-4e72-4a5b-a058-cdbc7837f4dc\",\"ef128343-2e29-4735-82f7-9d8d2231dcc2\",\"20675350-89f2-47e3-8e64-b04f2da6290b\",\"98a97cf1-8562-4828-9cf2-6f5823bf09fe\",\"1fde61af-e1e0-4027-ad21-1176ad212119\"]},{\"id\":8,\"content\":\"17\",\"referenceQuestionId\":[\"fcd14833-b79f-4cf4-9ee8-65b041a7daf1\",\"06b1a0df-eaef-4be9-a9da-9c2633e3a12a\",\"56628f4a-0384-426c-b849-81ab1671afa4\",\"f05b02dc-6393-41dd-a479-cc9c05d1da93\",\"bb50bb8b-4e72-4a5b-a058-cdbc7837f4dc\",\"ef128343-2e29-4735-82f7-9d8d2231dcc2\",\"20675350-89f2-47e3-8e64-b04f2da6290b\",\"98a97cf1-8562-4828-9cf2-6f5823bf09fe\",\"1fde61af-e1e0-4027-ad21-1176ad212119\"]},{\"id\":9,\"content\":\"18\",\"referenceQuestionId\":[\"fcd14833-b79f-4cf4-9ee8-65b041a7daf1\",\"06b1a0df-eaef-4be9-a9da-9c2633e3a12a\",\"56628f4a-0384-426c-b849-81ab1671afa4\",\"f05b02dc-6393-41dd-a479-cc9c05d1da93\",\"bb50bb8b-4e72-4a5b-a058-cdbc7837f4dc\",\"ef128343-2e29-4735-82f7-9d8d2231dcc2\",\"20675350-89f2-47e3-8e64-b04f2da6290b\",\"98a97cf1-8562-4828-9cf2-6f5823bf09fe\",\"1fde61af-e1e0-4027-ad21-1176ad212119\"]},{\"id\":10,\"content\":\"19\\u002B\",\"referenceQuestionId\":[\"1fde61af-e1e0-4027-ad21-1176ad212119\",\"5317ae1a-46db-4f25-9625-f9c28bd3c688\"]}]", 1, 3.2f, "Trẻ bao nhiêu tuổi" });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("1fde61af-e1e0-4027-ad21-1176ad212119"),
                columns: new[] { "DisplayLevel", "DisplayOrder" },
                values: new object[] { 2, 13f });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("20675350-89f2-47e3-8e64-b04f2da6290b"),
                columns: new[] { "AnswerStr", "DisplayLevel", "DisplayOrder" },
                values: new object[] { "[{\"id\":1,\"content\":\"C\\u00E1c bu\\u1ED5i t\\u1ED1i th\\u1EE9 Hai\",\"referenceQuestionId\":null},{\"id\":2,\"content\":\"C\\u00E1c bu\\u1ED5i t\\u1ED1i th\\u1EE9 Ba\",\"referenceQuestionId\":null},{\"id\":3,\"content\":\"C\\u00E1c bu\\u1ED5i t\\u1ED1i th\\u1EE9 T\\u01B0\",\"referenceQuestionId\":null},{\"id\":4,\"content\":\"C\\u00E1c bu\\u1ED5i t\\u1ED1i th\\u1EE9 N\\u0103m\",\"referenceQuestionId\":null},{\"id\":5,\"content\":\"C\\u00E1c bu\\u1ED5i t\\u1ED1i th\\u1EE9 S\\u00E1u\",\"referenceQuestionId\":null},{\"id\":6,\"content\":\"C\\u00E1c bu\\u1ED5i s\\u00E1ng th\\u1EE9 B\\u1EA3y\",\"referenceQuestionId\":null},{\"id\":7,\"content\":\"C\\u00E1c bu\\u1ED5i chi\\u1EC1u th\\u1EE9 B\\u1EA3y\",\"referenceQuestionId\":null},{\"id\":8,\"content\":\"C\\u00E1c bu\\u1ED5i t\\u1ED1i th\\u1EE9 B\\u1EA3y\",\"referenceQuestionId\":null},{\"id\":9,\"content\":\"C\\u00E1c bu\\u1ED5i s\\u00E1ng ch\\u1EE7 nh\\u1EADt\",\"referenceQuestionId\":null},{\"id\":10,\"content\":\"C\\u00E1c bu\\u1ED5i chi\\u1EC1u ch\\u1EE7 nh\\u1EADt\",\"referenceQuestionId\":null},{\"id\":11,\"content\":\"C\\u00E1c bu\\u1ED5i t\\u1ED1i ch\\u1EE7 nh\\u1EADt\",\"referenceQuestionId\":null}]", 2, 11f });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("2be9a620-143d-41f6-815b-2038c21a7b23"),
                columns: new[] { "AnswerStr", "DisplayLevel", "DisplayOrder" },
                values: new object[] { "{\"birthday\":null,\"ageGenderQuestions\":[{\"id\":1,\"content\":\"Male\"},{\"id\":2,\"content\":\"Female\"},{\"id\":3,\"content\":\"Other\"}]}", 1, 5f });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("492d8bb9-cdbe-42e7-aa16-35a1915c3621"),
                columns: new[] { "DisplayLevel", "DisplayOrder" },
                values: new object[] { 1, 1f });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("52548bc5-3536-478c-978b-05f98815bf31"),
                columns: new[] { "AnswerStr", "DisplayLevel", "DisplayOrder", "Question" },
                values: new object[] { "[{\"id\":1,\"content\":\"Nam\",\"referenceQuestionId\":null},{\"id\":2,\"content\":\"N\\u1EEF\",\"referenceQuestionId\":null},{\"id\":3,\"content\":\"Kh\\u00E1c\",\"referenceQuestionId\":null}]", 1, 3.3f, "Giới Tính của trẻ" });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("56628f4a-0384-426c-b849-81ab1671afa4"),
                columns: new[] { "AnswerStr", "DisplayLevel", "DisplayOrder" },
                values: new object[] { "[{\"id\":1,\"content\":\"1 (Kh\\u00F4ng s\\u1EB5n l\\u00F2ng ch\\u00FAt n\\u00E0o)\",\"referenceQuestionId\":null},{\"id\":2,\"content\":\"2\",\"referenceQuestionId\":null},{\"id\":3,\"content\":\"3\",\"referenceQuestionId\":null},{\"id\":4,\"content\":\"4\",\"referenceQuestionId\":null},{\"id\":5,\"content\":\"5\",\"referenceQuestionId\":null},{\"id\":6,\"content\":\"6\",\"referenceQuestionId\":null},{\"id\":7,\"content\":\"7\",\"referenceQuestionId\":null},{\"id\":8,\"content\":\"8\",\"referenceQuestionId\":null},{\"id\":9,\"content\":\"9\",\"referenceQuestionId\":null},{\"id\":10,\"content\":\"10 (R\\u1EA5t s\\u1EB5n s\\u00E0ng)\",\"referenceQuestionId\":null}]", 2, 7f });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("713d8bb9-cdbe-42e7-aa16-35a1915c3532"),
                columns: new[] { "DisplayLevel", "DisplayOrder" },
                values: new object[] { 1, 4f });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("721d8bb9-cdbe-42e7-aa16-35a1915c1123"),
                columns: new[] { "DisplayLevel", "DisplayOrder" },
                values: new object[] { 1, 3f });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("73ab87b1-c0ae-40c6-ba37-1cdce350563a"),
                columns: new[] { "AnswerStr", "DisplayLevel", "DisplayOrder" },
                values: new object[] { "[{\"id\":1,\"content\":\"C\\u00F3\",\"referenceQuestionId\":null},{\"id\":2,\"content\":\"Kh\\u00F4ng\",\"referenceQuestionId\":null}]", 2, 4f });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("763d8bb9-cdbe-42e7-aa16-35a1915c3512"),
                columns: new[] { "DisplayLevel", "DisplayOrder" },
                values: new object[] { 1, 2f });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("83a5b867-c3a1-47b9-91e3-a6fd6086501e"),
                columns: new[] { "AnswerStr", "DisplayLevel", "DisplayOrder" },
                values: new object[] { "[{\"id\":1,\"content\":\"C\\u00F3\",\"referenceQuestionId\":null},{\"id\":2,\"content\":\"Kh\\u00F4ng\",\"referenceQuestionId\":null}]", 2, 6f });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("9619b7d3-b2ab-4462-9044-b443ab79942a"),
                columns: new[] { "DisplayLevel", "DisplayOrder" },
                values: new object[] { 1, 2f });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("cedef58c-7d94-4efc-a030-0216b3034bef"),
                columns: new[] { "DisplayLevel", "DisplayOrder", "Question" },
                values: new object[] { 1, 3.1f, "Họ tên đầy đủ của trẻ là gì" });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("ee0e74f5-83ae-44dd-a7d0-0f7b650884f8"),
                columns: new[] { "DisplayLevel", "DisplayOrder" },
                values: new object[] { 1, 1.1f });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("f959d3d1-cab3-4fc6-8341-cb44bcbd3e30"),
                columns: new[] { "AnswerStr", "DisplayLevel", "DisplayOrder" },
                values: new object[] { "[{\"id\":1,\"content\":\"Ba \\u0110\\u00ECnh\",\"referenceQuestionId\":null},{\"id\":2,\"content\":\"Ho\\u00E0n Ki\\u1EBFm\",\"referenceQuestionId\":null},{\"id\":3,\"content\":\"T\\u00E2y H\\u1ED3\",\"referenceQuestionId\":null},{\"id\":4,\"content\":\"Long Bi\\u00EAn\",\"referenceQuestionId\":null},{\"id\":5,\"content\":\"C\\u1EA7u Gi\\u1EA5y\",\"referenceQuestionId\":null},{\"id\":6,\"content\":\"\\u0110\\u1ED1ng \\u0110a\",\"referenceQuestionId\":null},{\"id\":7,\"content\":\"Hai B\\u00E0 Tr\\u01B0ng\",\"referenceQuestionId\":null},{\"id\":8,\"content\":\"Ho\\u00E0ng Mai\",\"referenceQuestionId\":null},{\"id\":9,\"content\":\"Thanh Xu\\u00E2n\",\"referenceQuestionId\":null},{\"id\":10,\"content\":\"H\\u00E0 \\u0110\\u00F4ng\",\"referenceQuestionId\":null},{\"id\":11,\"content\":\"B\\u1EAFc T\\u1EEB Li\\u00EAm\",\"referenceQuestionId\":null},{\"id\":12,\"content\":\"Nam T\\u1EEB Li\\u00EAm\",\"referenceQuestionId\":null},{\"id\":13,\"content\":\"Ba V\\u00EC\",\"referenceQuestionId\":null},{\"id\":14,\"content\":\"Ch\\u01B0\\u01A1ng M\\u1EF9\",\"referenceQuestionId\":null},{\"id\":15,\"content\":\"\\u0110an Ph\\u01B0\\u1EE3ng\",\"referenceQuestionId\":null},{\"id\":16,\"content\":\"\\u0110\\u00F4ng Anh\",\"referenceQuestionId\":null},{\"id\":17,\"content\":\"Gia L\\u00E2m\",\"referenceQuestionId\":null},{\"id\":18,\"content\":\"Ho\\u00E0i \\u0110\\u1EE9c\",\"referenceQuestionId\":null},{\"id\":19,\"content\":\"M\\u00EA Linh\",\"referenceQuestionId\":null},{\"id\":20,\"content\":\"Ph\\u00FA Xuy\\u00EAn\",\"referenceQuestionId\":null},{\"id\":21,\"content\":\"Ph\\u00FAc Th\\u1ECD\",\"referenceQuestionId\":null},{\"id\":22,\"content\":\"Qu\\u1ED1c Oai\",\"referenceQuestionId\":null},{\"id\":23,\"content\":\"Th\\u1EA1ch Th\\u1EA5t\",\"referenceQuestionId\":null},{\"id\":24,\"content\":\"Thanh Oai\",\"referenceQuestionId\":null},{\"id\":25,\"content\":\"Thanh Tr\\u00EC\",\"referenceQuestionId\":null},{\"id\":26,\"content\":\"Th\\u01B0\\u1EDDng T\\u00EDn\",\"referenceQuestionId\":null},{\"id\":27,\"content\":\" \\u1EE8ng H\\u00F2a\",\"referenceQuestionId\":null},{\"id\":28,\"content\":\"kh\\u00E1c\",\"referenceQuestionId\":null}]", 1, 1.2f });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("fcd14833-b79f-4cf4-9ee8-65b041a7daf1"),
                columns: new[] { "AnswerStr", "DisplayLevel", "DisplayOrder" },
                values: new object[] { "[{\"id\":1,\"content\":\"C\\u00F3\",\"referenceQuestionId\":null},{\"id\":2,\"content\":\"Kh\\u00F4ng\",\"referenceQuestionId\":null}]", 2, 4f });

            migrationBuilder.InsertData(
                table: "SurveyQuestions",
                columns: new[] { "Id", "AnswerStr", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "Description", "DisplayLevel", "DisplayOrder", "Icon", "IsDeleted", "IsPilot", "Question", "Type", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[,]
                {
                    { new Guid("98a97cf1-8562-4828-9cf2-6f5823bf09fe"), "null", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "addd", 2, 12f, null, false, true, "Bạn Thích vị trí nào hơn cho các buổi họp tại chỗ?", "ShortAnswer", null, null, null },
                    { new Guid("bb50bb8b-4e72-4a5b-a058-cdbc7837f4dc"), "[{\"id\":1,\"content\":\"C\\u00E1c bu\\u1ED5i t\\u1ED1i th\\u1EE9 Hai\",\"referenceQuestionId\":null},{\"id\":2,\"content\":\"C\\u00E1c bu\\u1ED5i t\\u1ED1i th\\u1EE9 Ba\",\"referenceQuestionId\":null},{\"id\":3,\"content\":\"C\\u00E1c bu\\u1ED5i t\\u1ED1i th\\u1EE9 T\\u01B0\",\"referenceQuestionId\":null},{\"id\":4,\"content\":\"C\\u00E1c bu\\u1ED5i t\\u1ED1i th\\u1EE9 N\\u0103m\",\"referenceQuestionId\":null},{\"id\":5,\"content\":\"C\\u00E1c bu\\u1ED5i t\\u1ED1i th\\u1EE9 S\\u00E1u\",\"referenceQuestionId\":null},{\"id\":6,\"content\":\"C\\u00E1c bu\\u1ED5i s\\u00E1ng th\\u1EE9 B\\u1EA3y\",\"referenceQuestionId\":null},{\"id\":7,\"content\":\"C\\u00E1c bu\\u1ED5i chi\\u1EC1u th\\u1EE9 B\\u1EA3y\",\"referenceQuestionId\":null},{\"id\":8,\"content\":\"C\\u00E1c bu\\u1ED5i t\\u1ED1i th\\u1EE9 B\\u1EA3y\",\"referenceQuestionId\":null},{\"id\":9,\"content\":\"C\\u00E1c bu\\u1ED5i s\\u00E1ng ch\\u1EE7 nh\\u1EADt\",\"referenceQuestionId\":null},{\"id\":10,\"content\":\"C\\u00E1c bu\\u1ED5i chi\\u1EC1u ch\\u1EE7 nh\\u1EADt\",\"referenceQuestionId\":null},{\"id\":11,\"content\":\"C\\u00E1c bu\\u1ED5i t\\u1ED1i ch\\u1EE7 nh\\u1EADt\",\"referenceQuestionId\":null}]", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "addd", 2, 9f, null, false, true, "Ngày nào sau đây thuận tiện cho con bạn tham dự các buổi học tại chỗ trong thời gian thí điểm? Lưu ý rằng các buổi học tại chỗ sẽ chỉ được tổ chức khoảng một lần mỗi tháng.(Chọn nhiều phương án)", "CheckList", null, null, null },
                    { new Guid("ef128343-2e29-4735-82f7-9d8d2231dcc2"), "[{\"id\":1,\"content\":\"1 (Kh\\u00F4ng s\\u1EB5n l\\u00F2ng ch\\u00FAt n\\u00E0o)\",\"referenceQuestionId\":null},{\"id\":2,\"content\":\"2\",\"referenceQuestionId\":null},{\"id\":3,\"content\":\"3\",\"referenceQuestionId\":null},{\"id\":4,\"content\":\"4\",\"referenceQuestionId\":null},{\"id\":5,\"content\":\"5\",\"referenceQuestionId\":null},{\"id\":6,\"content\":\"6\",\"referenceQuestionId\":null},{\"id\":7,\"content\":\"7\",\"referenceQuestionId\":null},{\"id\":8,\"content\":\"8\",\"referenceQuestionId\":null},{\"id\":9,\"content\":\"9\",\"referenceQuestionId\":null},{\"id\":10,\"content\":\"10 (R\\u1EA5t s\\u1EB5n s\\u00E0ng)\",\"referenceQuestionId\":null}]", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "addd", 2, 10f, null, false, true, "Bạn có sẵn lòng tham dự các buổi họp tại chỗ để tham gia các cuộc phóng vấn phụ huynh và các buổi họp nhóm tập trung không?", "MultipleChoiceHorizontal", null, null, null },
                    { new Guid("f05b02dc-6393-41dd-a479-cc9c05d1da93"), "[{\"id\":1,\"content\":\"33 L\\u1EA1c Trung\",\"referenceQuestionId\":null},{\"id\":2,\"content\":\"125 Ho\\u00E0ng Ng\\u00E2n\",\"referenceQuestionId\":null},{\"id\":3,\"content\":\"T\\u00F4i \\u1ED5n v\\u1EDBi c\\u1EA3 hai\",\"referenceQuestionId\":null}]", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "addd", 2, 8f, null, false, true, "Bạn thích vị trí nào hơn cho các buổi họp tại chỗ?", "ShortAnswer", null, null, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("98a97cf1-8562-4828-9cf2-6f5823bf09fe"));

            migrationBuilder.DeleteData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("bb50bb8b-4e72-4a5b-a058-cdbc7837f4dc"));

            migrationBuilder.DeleteData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("ef128343-2e29-4735-82f7-9d8d2231dcc2"));

            migrationBuilder.DeleteData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("f05b02dc-6393-41dd-a479-cc9c05d1da93"));

            migrationBuilder.DropColumn(
                name: "DisplayLevel",
                table: "SurveyQuestions");

            migrationBuilder.AlterColumn<int>(
                name: "DisplayOrder",
                table: "SurveyQuestions",
                type: "int",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("06b1a0df-eaef-4be9-a9da-9c2633e3a12a"),
                columns: new[] { "AnswerStr", "DisplayOrder" },
                values: new object[] { "[{\"id\":1,\"content\":\"Laptop\"},{\"id\":2,\"content\":\"PC\"},{\"id\":3,\"content\":\"Both\"},{\"id\":4,\"content\":\"No\"}]", 13 });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("0f9df809-9ae9-4e8c-891d-ec645483f290"),
                columns: new[] { "AnswerStr", "DisplayOrder", "Question" },
                values: new object[] { "[{\"id\":1,\"age\":\"\\u003C11\"},{\"id\":2,\"age\":\"11\"},{\"id\":3,\"age\":\"12\"},{\"id\":4,\"age\":\"13\"},{\"id\":5,\"age\":\"14\"},{\"id\":6,\"age\":\"15\"},{\"id\":7,\"age\":\"16\"},{\"id\":8,\"age\":\"17\"},{\"id\":9,\"age\":\"18\"},{\"id\":10,\"age\":\"19\\u002B\"}]", 9, "Xác định độ tuổi và giới tính" });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("1fde61af-e1e0-4027-ad21-1176ad212119"),
                column: "DisplayOrder",
                value: 18);

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("20675350-89f2-47e3-8e64-b04f2da6290b"),
                columns: new[] { "AnswerStr", "DisplayOrder" },
                values: new object[] { "[{\"id\":1,\"content\":\"Monday evenings\"},{\"id\":2,\"content\":\"Tuesday evenings\"},{\"id\":3,\"content\":\"Wednesday evenings\"},{\"id\":4,\"content\":\"Thursday evenings\"},{\"id\":5,\"content\":\"Friday evenings\"},{\"id\":6,\"content\":\"Saturday mornings\"},{\"id\":7,\"content\":\"Saturday afternoons\"},{\"id\":8,\"content\":\"Saturday evenings\"},{\"id\":9,\"content\":\"Sunday mornings\"},{\"id\":10,\"content\":\"Sunday afternoons\"},{\"id\":11,\"content\":\"Sunday evenings\"}]", 16 });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("2be9a620-143d-41f6-815b-2038c21a7b23"),
                columns: new[] { "AnswerStr", "DisplayOrder" },
                values: new object[] { "{\"birthday\":null,\"ageGenderQuestions\":[{\"id\":1,\"gender\":\"Male\"},{\"id\":2,\"gender\":\"Female\"},{\"id\":3,\"gender\":\"Other\"}]}", 5 });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("492d8bb9-cdbe-42e7-aa16-35a1915c3621"),
                column: "DisplayOrder",
                value: 1);

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("52548bc5-3536-478c-978b-05f98815bf31"),
                columns: new[] { "AnswerStr", "DisplayOrder", "Question" },
                values: new object[] { "[{\"id\":1,\"gender\":\"Male\"},{\"id\":2,\"gender\":\"Female\"},{\"id\":3,\"gender\":\"Other\"}]", 10, "Xác định độ tuổi và giới tính" });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("56628f4a-0384-426c-b849-81ab1671afa4"),
                columns: new[] { "AnswerStr", "DisplayOrder" },
                values: new object[] { "[{\"id\":1,\"content\":\"1 (Not willing at all)\"},{\"id\":2,\"content\":\"2\"},{\"id\":3,\"content\":\"3\"},{\"id\":4,\"content\":\"4\"},{\"id\":5,\"content\":\"5\"},{\"id\":6,\"content\":\"6\"},{\"id\":7,\"content\":\"7\"},{\"id\":8,\"content\":\"8\"},{\"id\":9,\"content\":\"9\"},{\"id\":10,\"content\":\"10 (Very Willing)\"}]", 15 });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("713d8bb9-cdbe-42e7-aa16-35a1915c3532"),
                column: "DisplayOrder",
                value: 4);

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("721d8bb9-cdbe-42e7-aa16-35a1915c1123"),
                column: "DisplayOrder",
                value: 3);

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("73ab87b1-c0ae-40c6-ba37-1cdce350563a"),
                columns: new[] { "AnswerStr", "DisplayOrder" },
                values: new object[] { "[{\"id\":1,\"content\":\"Yes\"},{\"id\":2,\"content\":\"No\"}]", 11 });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("763d8bb9-cdbe-42e7-aa16-35a1915c3512"),
                column: "DisplayOrder",
                value: 2);

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("83a5b867-c3a1-47b9-91e3-a6fd6086501e"),
                columns: new[] { "AnswerStr", "DisplayOrder" },
                values: new object[] { "[{\"id\":1,\"content\":\"Yes\"},{\"id\":2,\"content\":\"No\"}]", 14 });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("9619b7d3-b2ab-4462-9044-b443ab79942a"),
                column: "DisplayOrder",
                value: 7);

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("cedef58c-7d94-4efc-a030-0216b3034bef"),
                columns: new[] { "DisplayOrder", "Question" },
                values: new object[] { 8, "Họ tên đầy đủ của trẻ nhà bạn là gì" });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("ee0e74f5-83ae-44dd-a7d0-0f7b650884f8"),
                column: "DisplayOrder",
                value: 6);

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("f959d3d1-cab3-4fc6-8341-cb44bcbd3e30"),
                columns: new[] { "AnswerStr", "DisplayOrder" },
                values: new object[] { "[{\"id\":1,\"district\":\"Ba \\u0110\\u00ECnh\"},{\"id\":2,\"district\":\"Ho\\u00E0n Ki\\u1EBFm\"},{\"id\":3,\"district\":\"T\\u00E2y H\\u1ED3\"},{\"id\":4,\"district\":\"Long Bi\\u00EAn\"},{\"id\":5,\"district\":\"C\\u1EA7u Gi\\u1EA5y\"},{\"id\":6,\"district\":\"\\u0110\\u1ED1ng \\u0110a\"},{\"id\":7,\"district\":\"Hai B\\u00E0 Tr\\u01B0ng\"},{\"id\":8,\"district\":\"Ho\\u00E0ng Mai\"},{\"id\":9,\"district\":\"Thanh Xu\\u00E2n\"},{\"id\":10,\"district\":\"H\\u00E0 \\u0110\\u00F4ng\"},{\"id\":11,\"district\":\"B\\u1EAFc T\\u1EEB Li\\u00EAm\"},{\"id\":12,\"district\":\"Nam T\\u1EEB Li\\u00EAm\"},{\"id\":13,\"district\":\"Ba V\\u00EC\"},{\"id\":14,\"district\":\"Ch\\u01B0\\u01A1ng M\\u1EF9\"},{\"id\":15,\"district\":\"\\u0110an Ph\\u01B0\\u1EE3ng\"},{\"id\":16,\"district\":\"\\u0110\\u00F4ng Anh\"},{\"id\":17,\"district\":\"Gia L\\u00E2m\"},{\"id\":18,\"district\":\"Ho\\u00E0i \\u0110\\u1EE9c\"},{\"id\":19,\"district\":\"M\\u00EA Linh\"},{\"id\":20,\"district\":\"Ph\\u00FA Xuy\\u00EAn\"},{\"id\":21,\"district\":\"Ph\\u00FAc Th\\u1ECD\"},{\"id\":22,\"district\":\"Qu\\u1ED1c Oai\"},{\"id\":23,\"district\":\"Th\\u1EA1ch Th\\u1EA5t\"},{\"id\":24,\"district\":\"Thanh Oai\"},{\"id\":25,\"district\":\"Thanh Tr\\u00EC\"},{\"id\":26,\"district\":\"Th\\u01B0\\u1EDDng T\\u00EDn\"},{\"id\":27,\"district\":\" \\u1EE8ng H\\u00F2a\"},{\"id\":28,\"district\":\"kh\\u00E1c\"}]", 7 });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("fcd14833-b79f-4cf4-9ee8-65b041a7daf1"),
                columns: new[] { "AnswerStr", "DisplayOrder" },
                values: new object[] { "[{\"id\":1,\"content\":\"Yes\"},{\"id\":2,\"content\":\"No\"}]", 12 });

            migrationBuilder.InsertData(
                table: "SurveyQuestions",
                columns: new[] { "Id", "AnswerStr", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "Description", "DisplayOrder", "Icon", "IsDeleted", "IsPilot", "Question", "Type", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[] { new Guid("5317ae1a-46db-4f25-9625-f9c28bd3c688"), "null", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "addd", 17, null, false, true, "Bạn biết đến FSEL Pilot bằng cách nào?", "ShortAnswer", null, null, null });
        }
    }
}
