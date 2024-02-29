using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Fsel.Interaction.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSurveyQuestionV2Table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("06b1a0df-eaef-4be9-a9da-9c2633e3a12a"));

            migrationBuilder.DeleteData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("0f9df809-9ae9-4e8c-891d-ec645483f290"));

            migrationBuilder.DeleteData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("1fde61af-e1e0-4027-ad21-1176ad212119"));

            migrationBuilder.DeleteData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("20675350-89f2-47e3-8e64-b04f2da6290b"));

            migrationBuilder.DeleteData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("52548bc5-3536-478c-978b-05f98815bf31"));

            migrationBuilder.DeleteData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("56628f4a-0384-426c-b849-81ab1671afa4"));

            migrationBuilder.DeleteData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("651414ad-cbcf-462a-84b9-daf83880b25a"));

            migrationBuilder.DeleteData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("73ab87b1-c0ae-40c6-ba37-1cdce350563a"));

            migrationBuilder.DeleteData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("763d8bb9-cdbe-42e7-aa16-35a1915c3512"));

            migrationBuilder.DeleteData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("83a5b867-c3a1-47b9-91e3-a6fd6086501e"));

            migrationBuilder.DeleteData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("9619b7d3-b2ab-4462-9044-b443ab79942a"));

            migrationBuilder.DeleteData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("98a97cf1-8562-4828-9cf2-6f5823bf09fe"));

            migrationBuilder.DeleteData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("a6f6b5dc-c72a-496c-859c-2fdc9d97f147"));

            migrationBuilder.DeleteData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("bb50bb8b-4e72-4a5b-a058-cdbc7837f4dc"));

            migrationBuilder.DeleteData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("cedef58c-7d94-4efc-a030-0216b3034bef"));

            migrationBuilder.DeleteData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("ee0e74f5-83ae-44dd-a7d0-0f7b650884f8"));

            migrationBuilder.DeleteData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("ef128343-2e29-4735-82f7-9d8d2231dcc2"));

            migrationBuilder.DeleteData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("f05b02dc-6393-41dd-a479-cc9c05d1da93"));

            migrationBuilder.DeleteData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("f959d3d1-cab3-4fc6-8341-cb44bcbd3e30"));

            migrationBuilder.DropColumn(
                name: "IsPilot",
                table: "SurveyQuestions");

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

            migrationBuilder.InsertData(
                table: "SurveyQuestions",
                columns: new[] { "Id", "AnswerStr", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "Description", "DisplayLevel", "DisplayOrder", "Icon", "IsDeleted", "Question", "Type", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[] { new Guid("b223125a-a4e1-4e10-b4dd-cfcd747d74c5"), "null", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "addd", 1, 4f, "wideword.png", false, "Trường học của bạn", "Location", null, null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("b223125a-a4e1-4e10-b4dd-cfcd747d74c5"));

            migrationBuilder.AddColumn<bool>(
                name: "IsPilot",
                table: "SurveyQuestions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("2be9a620-143d-41f6-815b-2038c21a7b23"),
                column: "IsPilot",
                value: false);

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("492d8bb9-cdbe-42e7-aa16-35a1915c3621"),
                column: "IsPilot",
                value: false);

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("713d8bb9-cdbe-42e7-aa16-35a1915c3532"),
                columns: new[] { "DisplayOrder", "IsPilot" },
                values: new object[] { 4f, false });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("721d8bb9-cdbe-42e7-aa16-35a1915c1123"),
                columns: new[] { "DisplayOrder", "IsPilot" },
                values: new object[] { 3f, false });

            migrationBuilder.InsertData(
                table: "SurveyQuestions",
                columns: new[] { "Id", "AnswerStr", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "Description", "DisplayLevel", "DisplayOrder", "Icon", "IsDeleted", "IsPilot", "Question", "Type", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[,]
                {
                    { new Guid("06b1a0df-eaef-4be9-a9da-9c2633e3a12a"), "[{\"id\":1,\"content\":\"M\\u00E1y t\\u00EDnh x\\u00E1ch tay\",\"referenceQuestionId\":\"83a5b867-c3a1-47b9-91e3-a6fd6086501e\"},{\"id\":2,\"content\":\"M\\u00E1y t\\u00EDnh \\u0111\\u1EC3 b\\u00E0n\",\"referenceQuestionId\":null},{\"id\":3,\"content\":\"C\\u1EA3 2\",\"referenceQuestionId\":\"83a5b867-c3a1-47b9-91e3-a6fd6086501e\"},{\"id\":4,\"content\":\"No\",\"referenceQuestionId\":null}]", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Câu trả lời của bạn", 2, 5f, null, false, true, "Bạn có máy tính để bàn hoặc máy tính xách tay ở nhà để con bạn có thể sử dụng để truy cập khóa học không?", "MultipleChoiceVertical", null, null, null },
                    { new Guid("0f9df809-9ae9-4e8c-891d-ec645483f290"), "[{\"id\":1,\"min\":0,\"max\":10,\"referenceQuestionId\":[\"98a97cf1-8562-4828-9cf2-6f5823bf09fe\",\"1fde61af-e1e0-4027-ad21-1176ad212119\"]},{\"id\":2,\"min\":11,\"max\":15,\"referenceQuestionId\":[\"73ab87b1-c0ae-40c6-ba37-1cdce350563a\",\"06b1a0df-eaef-4be9-a9da-9c2633e3a12a\",\"56628f4a-0384-426c-b849-81ab1671afa4\",\"f05b02dc-6393-41dd-a479-cc9c05d1da93\",\"651414ad-cbcf-462a-84b9-daf83880b25a\",\"ef128343-2e29-4735-82f7-9d8d2231dcc2\",\"20675350-89f2-47e3-8e64-b04f2da6290b\",\"98a97cf1-8562-4828-9cf2-6f5823bf09fe\",\"1fde61af-e1e0-4027-ad21-1176ad212119\"]},{\"id\":7,\"min\":16,\"max\":18,\"referenceQuestionId\":[\"a6f6b5dc-c72a-496c-859c-2fdc9d97f147\",\"06b1a0df-eaef-4be9-a9da-9c2633e3a12a\",\"56628f4a-0384-426c-b849-81ab1671afa4\",\"f05b02dc-6393-41dd-a479-cc9c05d1da93\",\"bb50bb8b-4e72-4a5b-a058-cdbc7837f4dc\",\"ef128343-2e29-4735-82f7-9d8d2231dcc2\",\"20675350-89f2-47e3-8e64-b04f2da6290b\",\"98a97cf1-8562-4828-9cf2-6f5823bf09fe\",\"1fde61af-e1e0-4027-ad21-1176ad212119\"]},{\"id\":10,\"min\":19,\"max\":1000,\"referenceQuestionId\":[\"98a97cf1-8562-4828-9cf2-6f5823bf09fe\",\"1fde61af-e1e0-4027-ad21-1176ad212119\"]}]", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Năm sinh", 1, 3.2f, null, false, true, "Năm sinh của trẻ", "YearInput", null, null, null },
                    { new Guid("1fde61af-e1e0-4027-ad21-1176ad212119"), "null", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Câu trả lời của bạn", 2, 13f, null, false, true, "Tại sao bạn muốn đăng ký chương trình học trải nghiệm của FSEL?", "ShortAnswer", null, null, null },
                    { new Guid("20675350-89f2-47e3-8e64-b04f2da6290b"), "[{\"id\":1,\"content\":\"C\\u00E1c bu\\u1ED5i t\\u1ED1i th\\u1EE9 Hai\",\"referenceQuestionId\":null},{\"id\":2,\"content\":\"C\\u00E1c bu\\u1ED5i t\\u1ED1i th\\u1EE9 Ba\",\"referenceQuestionId\":null},{\"id\":3,\"content\":\"C\\u00E1c bu\\u1ED5i t\\u1ED1i th\\u1EE9 T\\u01B0\",\"referenceQuestionId\":null},{\"id\":4,\"content\":\"C\\u00E1c bu\\u1ED5i t\\u1ED1i th\\u1EE9 N\\u0103m\",\"referenceQuestionId\":null},{\"id\":5,\"content\":\"C\\u00E1c bu\\u1ED5i t\\u1ED1i th\\u1EE9 S\\u00E1u\",\"referenceQuestionId\":null},{\"id\":6,\"content\":\"C\\u00E1c bu\\u1ED5i s\\u00E1ng th\\u1EE9 B\\u1EA3y\",\"referenceQuestionId\":null},{\"id\":7,\"content\":\"C\\u00E1c bu\\u1ED5i chi\\u1EC1u th\\u1EE9 B\\u1EA3y\",\"referenceQuestionId\":null},{\"id\":8,\"content\":\"C\\u00E1c bu\\u1ED5i t\\u1ED1i th\\u1EE9 B\\u1EA3y\",\"referenceQuestionId\":null},{\"id\":9,\"content\":\"C\\u00E1c bu\\u1ED5i s\\u00E1ng ch\\u1EE7 nh\\u1EADt\",\"referenceQuestionId\":null},{\"id\":10,\"content\":\"C\\u00E1c bu\\u1ED5i chi\\u1EC1u ch\\u1EE7 nh\\u1EADt\",\"referenceQuestionId\":null},{\"id\":11,\"content\":\"C\\u00E1c bu\\u1ED5i t\\u1ED1i ch\\u1EE7 nh\\u1EADt\",\"referenceQuestionId\":null}]", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Câu trả lời của bạn", 2, 11f, null, false, true, "Ngày nào sau đây thuận tiện cho bạn tham dự các buổi họp tại trung tâm trong giai đoạn thử nghiệm? (Chọn nhiều phương án)", "CheckList", null, null, null },
                    { new Guid("52548bc5-3536-478c-978b-05f98815bf31"), "[{\"id\":1,\"content\":\"Nam\",\"referenceQuestionId\":null},{\"id\":2,\"content\":\"N\\u1EEF\",\"referenceQuestionId\":null},{\"id\":3,\"content\":\"Kh\\u00E1c\",\"referenceQuestionId\":null}]", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Giới tính", 1, 3.3f, null, false, true, "Giới Tính của trẻ", "MultipleChoiceVertical", null, null, null },
                    { new Guid("56628f4a-0384-426c-b849-81ab1671afa4"), "[{\"id\":1,\"content\":\"1 (Kh\\u00F4ng s\\u1EB5n l\\u00F2ng ch\\u00FAt n\\u00E0o)\",\"referenceQuestionId\":null},{\"id\":2,\"content\":\"2\",\"referenceQuestionId\":null},{\"id\":3,\"content\":\"3\",\"referenceQuestionId\":null},{\"id\":4,\"content\":\"4\",\"referenceQuestionId\":null},{\"id\":5,\"content\":\"5\",\"referenceQuestionId\":null},{\"id\":6,\"content\":\"6\",\"referenceQuestionId\":null},{\"id\":7,\"content\":\"7\",\"referenceQuestionId\":null},{\"id\":8,\"content\":\"8\",\"referenceQuestionId\":null},{\"id\":9,\"content\":\"9\",\"referenceQuestionId\":null},{\"id\":10,\"content\":\"10 (R\\u1EA5t s\\u1EB5n s\\u00E0ng)\",\"referenceQuestionId\":null}]", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Câu trả lời của bạn", 2, 7f, null, false, true, "Chúng tôi sẽ tổ chức 2 buổi gặp mặt trực tiếp (2 tiếng/buổi) tại trung tâm (33 Lạc Trung hoặc 125 Hoàng Ngân) để lấy ý kiến phản hồi của các bạn trong giai đoạn thử nghiệm. Bạn sẵn sàng đưa con mình tham dự những buổi học này ở mức độ nào?", "MultipleChoiceHorizontal", null, null, null },
                    { new Guid("651414ad-cbcf-462a-84b9-daf83880b25a"), "[{\"id\":1,\"content\":\"C\\u00E1c bu\\u1ED5i t\\u1ED1i th\\u1EE9 Hai\",\"referenceQuestionId\":null},{\"id\":2,\"content\":\"C\\u00E1c bu\\u1ED5i t\\u1ED1i th\\u1EE9 Ba\",\"referenceQuestionId\":null},{\"id\":3,\"content\":\"C\\u00E1c bu\\u1ED5i t\\u1ED1i th\\u1EE9 T\\u01B0\",\"referenceQuestionId\":null},{\"id\":4,\"content\":\"C\\u00E1c bu\\u1ED5i t\\u1ED1i th\\u1EE9 N\\u0103m\",\"referenceQuestionId\":null},{\"id\":5,\"content\":\"C\\u00E1c bu\\u1ED5i t\\u1ED1i th\\u1EE9 S\\u00E1u\",\"referenceQuestionId\":null},{\"id\":6,\"content\":\"C\\u00E1c bu\\u1ED5i s\\u00E1ng th\\u1EE9 B\\u1EA3y\",\"referenceQuestionId\":null},{\"id\":7,\"content\":\"C\\u00E1c bu\\u1ED5i chi\\u1EC1u th\\u1EE9 B\\u1EA3y\",\"referenceQuestionId\":null},{\"id\":8,\"content\":\"C\\u00E1c bu\\u1ED5i t\\u1ED1i th\\u1EE9 B\\u1EA3y\",\"referenceQuestionId\":null},{\"id\":9,\"content\":\"C\\u00E1c bu\\u1ED5i s\\u00E1ng ch\\u1EE7 nh\\u1EADt\",\"referenceQuestionId\":null},{\"id\":10,\"content\":\"C\\u00E1c bu\\u1ED5i chi\\u1EC1u ch\\u1EE7 nh\\u1EADt\",\"referenceQuestionId\":null},{\"id\":11,\"content\":\"C\\u00E1c bu\\u1ED5i t\\u1ED1i ch\\u1EE7 nh\\u1EADt\",\"referenceQuestionId\":null}]", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Câu trả lời của bạn", 2, 9f, null, false, true, "Ngày nào sau đây thuận tiện cho con bạn tham dự các buổi học trong giai đoạn thử nghiệm? Lưu ý rằng các buổi học sẽ chỉ được tổ chức khoảng một lần mỗi 3 tháng. (Chọn nhiều phương án)", "CheckList", null, null, null },
                    { new Guid("73ab87b1-c0ae-40c6-ba37-1cdce350563a"), "[{\"id\":1,\"content\":\"C\\u00F3\",\"referenceQuestionId\":null},{\"id\":2,\"content\":\"Kh\\u00F4ng\",\"referenceQuestionId\":null}]", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Câu trả lời của bạn", 2, 4f, null, false, true, "Bạn nhà có thể cam kết hoàn thành khóa học 6 tháng với tốc độ 3 buổi học mỗi tuần (7,5 giờ mỗi tuần) từ tháng 11 năm 2023 đến tháng 4 năm 2024 không?", "MultipleChoiceVertical", null, null, null },
                    { new Guid("763d8bb9-cdbe-42e7-aa16-35a1915c3512"), "[{\"id\":1,\"content\":\"B\\u1EA1n \\u0111\\u00E3 bi\\u1EBFt m\\u1ED9t ch\\u00FAt Ti\\u1EBFng Anh\"},{\"id\":2,\"content\":\"\\u0110\\u00E2y l\\u00E0 l\\u1EA7n \\u0111\\u1EA7u b\\u1EA1n h\\u1ECDc Ti\\u1EBFng Anh\"}]", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "addd", 1, 2f, "addd", false, false, "Chọn hướng đi của bạn", "YourDirection", null, null, null },
                    { new Guid("83a5b867-c3a1-47b9-91e3-a6fd6086501e"), "[{\"id\":1,\"content\":\"C\\u00F3\",\"referenceQuestionId\":null},{\"id\":2,\"content\":\"Kh\\u00F4ng\",\"referenceQuestionId\":null}]", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Câu trả lời của bạn", 2, 6f, null, false, true, "Con bạn có thể mang máy tính xách tay đến các buổi học tại trung tâm chúng tôi để tham gia khóa học không?", "MultipleChoiceVertical", null, null, null },
                    { new Guid("9619b7d3-b2ab-4462-9044-b443ab79942a"), "null", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "+84", 1, 2f, null, false, true, "Số điện thoại của bạn là gì", "ShortAnswer", null, null, null },
                    { new Guid("98a97cf1-8562-4828-9cf2-6f5823bf09fe"), "null", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Câu trả lời của bạn", 2, 12f, null, false, true, "Bạn biết đến chương trình học trải nghiệm của FSEL bằng cách nào?", "ShortAnswer", null, null, null },
                    { new Guid("a6f6b5dc-c72a-496c-859c-2fdc9d97f147"), "[{\"id\":1,\"content\":\"C\\u00F3\",\"referenceQuestionId\":null},{\"id\":2,\"content\":\"Kh\\u00F4ng\",\"referenceQuestionId\":null}]", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Câu trả lời của bạn", 2, 4f, null, false, true, "Bạn nhà có thể cam kết hoàn thành khóa học 4 tháng với tốc độ 2 buổi học mỗi tuần (5 giờ mỗi tuần) từ tháng 11 năm 2023 đến tháng 2 năm 2024 không?", "MultipleChoiceVertical", null, null, null },
                    { new Guid("bb50bb8b-4e72-4a5b-a058-cdbc7837f4dc"), "[{\"id\":1,\"content\":\"C\\u00E1c bu\\u1ED5i t\\u1ED1i th\\u1EE9 Hai\",\"referenceQuestionId\":null},{\"id\":2,\"content\":\"C\\u00E1c bu\\u1ED5i t\\u1ED1i th\\u1EE9 Ba\",\"referenceQuestionId\":null},{\"id\":3,\"content\":\"C\\u00E1c bu\\u1ED5i t\\u1ED1i th\\u1EE9 T\\u01B0\",\"referenceQuestionId\":null},{\"id\":4,\"content\":\"C\\u00E1c bu\\u1ED5i t\\u1ED1i th\\u1EE9 N\\u0103m\",\"referenceQuestionId\":null},{\"id\":5,\"content\":\"C\\u00E1c bu\\u1ED5i t\\u1ED1i th\\u1EE9 S\\u00E1u\",\"referenceQuestionId\":null},{\"id\":6,\"content\":\"C\\u00E1c bu\\u1ED5i s\\u00E1ng th\\u1EE9 B\\u1EA3y\",\"referenceQuestionId\":null},{\"id\":7,\"content\":\"C\\u00E1c bu\\u1ED5i chi\\u1EC1u th\\u1EE9 B\\u1EA3y\",\"referenceQuestionId\":null},{\"id\":8,\"content\":\"C\\u00E1c bu\\u1ED5i t\\u1ED1i th\\u1EE9 B\\u1EA3y\",\"referenceQuestionId\":null},{\"id\":9,\"content\":\"C\\u00E1c bu\\u1ED5i s\\u00E1ng ch\\u1EE7 nh\\u1EADt\",\"referenceQuestionId\":null},{\"id\":10,\"content\":\"C\\u00E1c bu\\u1ED5i chi\\u1EC1u ch\\u1EE7 nh\\u1EADt\",\"referenceQuestionId\":null},{\"id\":11,\"content\":\"C\\u00E1c bu\\u1ED5i t\\u1ED1i ch\\u1EE7 nh\\u1EADt\",\"referenceQuestionId\":null}]", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Câu trả lời của bạn", 2, 9f, null, false, true, "Ngày nào sau đây thuận tiện cho con bạn tham dự các buổi học trong giai đoạn thử nghiệm? Lưu ý rằng các buổi học sẽ chỉ được tổ chức khoảng một lần mỗi 2 tháng. (Chọn nhiều phương án)", "CheckList", null, null, null },
                    { new Guid("cedef58c-7d94-4efc-a030-0216b3034bef"), "null", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Họ tên", 1, 3.1f, null, false, true, "Họ tên đầy đủ của trẻ là gì", "ShortAnswer", null, null, null },
                    { new Guid("ee0e74f5-83ae-44dd-a7d0-0f7b650884f8"), "null", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Câu trả lời của bạn", 1, 1.1f, null, false, true, "Họ tên", "ShortAnswer", null, null, null },
                    { new Guid("ef128343-2e29-4735-82f7-9d8d2231dcc2"), "[{\"id\":1,\"content\":\"1 (Kh\\u00F4ng s\\u1EB5n s\\u00E0ng)\",\"referenceQuestionId\":null},{\"id\":2,\"content\":\"2\",\"referenceQuestionId\":null},{\"id\":3,\"content\":\"3\",\"referenceQuestionId\":null},{\"id\":4,\"content\":\"4\",\"referenceQuestionId\":null},{\"id\":5,\"content\":\"5\",\"referenceQuestionId\":null},{\"id\":6,\"content\":\"6\",\"referenceQuestionId\":null},{\"id\":7,\"content\":\"7\",\"referenceQuestionId\":null},{\"id\":8,\"content\":\"8\",\"referenceQuestionId\":null},{\"id\":9,\"content\":\"9\",\"referenceQuestionId\":null},{\"id\":10,\"content\":\"10 (R\\u1EA5t s\\u1EB5n s\\u00E0ng)\",\"referenceQuestionId\":null}]", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Câu trả lời của bạn", 2, 10f, null, false, true, "Bạn có sẵn sàng tham dự các buổi họp tại trung tâm để tham gia các cuộc phỏng vấn và các buổi họp nhóm tập trung cho phụ huynh không?", "MultipleChoiceHorizontal", null, null, null },
                    { new Guid("f05b02dc-6393-41dd-a479-cc9c05d1da93"), "[{\"id\":1,\"content\":\"33 L\\u1EA1c Trung\",\"referenceQuestionId\":null},{\"id\":2,\"content\":\"125 Ho\\u00E0ng Ng\\u00E2n\",\"referenceQuestionId\":null},{\"id\":3,\"content\":\"T\\u00F4i \\u1ED5n v\\u1EDBi c\\u1EA3 hai\",\"referenceQuestionId\":null}]", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Câu trả lời của bạn", 2, 8f, null, false, true, "Bạn mong muốn các buổi họp được tổ chức tại trung tâm nào hơn?", "MultipleChoiceVertical", null, null, null },
                    { new Guid("f959d3d1-cab3-4fc6-8341-cb44bcbd3e30"), "[{\"id\":1,\"content\":\"Ba \\u0110\\u00ECnh\",\"referenceQuestionId\":null},{\"id\":2,\"content\":\"Ho\\u00E0n Ki\\u1EBFm\",\"referenceQuestionId\":null},{\"id\":3,\"content\":\"T\\u00E2y H\\u1ED3\",\"referenceQuestionId\":null},{\"id\":4,\"content\":\"Long Bi\\u00EAn\",\"referenceQuestionId\":null},{\"id\":5,\"content\":\"C\\u1EA7u Gi\\u1EA5y\",\"referenceQuestionId\":null},{\"id\":6,\"content\":\"\\u0110\\u1ED1ng \\u0110a\",\"referenceQuestionId\":null},{\"id\":7,\"content\":\"Hai B\\u00E0 Tr\\u01B0ng\",\"referenceQuestionId\":null},{\"id\":8,\"content\":\"Ho\\u00E0ng Mai\",\"referenceQuestionId\":null},{\"id\":9,\"content\":\"Thanh Xu\\u00E2n\",\"referenceQuestionId\":null},{\"id\":10,\"content\":\"H\\u00E0 \\u0110\\u00F4ng\",\"referenceQuestionId\":null},{\"id\":11,\"content\":\"B\\u1EAFc T\\u1EEB Li\\u00EAm\",\"referenceQuestionId\":null},{\"id\":12,\"content\":\"Nam T\\u1EEB Li\\u00EAm\",\"referenceQuestionId\":null},{\"id\":13,\"content\":\"Ba V\\u00EC\",\"referenceQuestionId\":null},{\"id\":14,\"content\":\"Ch\\u01B0\\u01A1ng M\\u1EF9\",\"referenceQuestionId\":null},{\"id\":15,\"content\":\"\\u0110an Ph\\u01B0\\u1EE3ng\",\"referenceQuestionId\":null},{\"id\":16,\"content\":\"\\u0110\\u00F4ng Anh\",\"referenceQuestionId\":null},{\"id\":17,\"content\":\"Gia L\\u00E2m\",\"referenceQuestionId\":null},{\"id\":18,\"content\":\"Ho\\u00E0i \\u0110\\u1EE9c\",\"referenceQuestionId\":null},{\"id\":19,\"content\":\"M\\u00EA Linh\",\"referenceQuestionId\":null},{\"id\":20,\"content\":\"Ph\\u00FA Xuy\\u00EAn\",\"referenceQuestionId\":null},{\"id\":21,\"content\":\"Ph\\u00FAc Th\\u1ECD\",\"referenceQuestionId\":null},{\"id\":22,\"content\":\"Qu\\u1ED1c Oai\",\"referenceQuestionId\":null},{\"id\":23,\"content\":\"Th\\u1EA1ch Th\\u1EA5t\",\"referenceQuestionId\":null},{\"id\":24,\"content\":\"Thanh Oai\",\"referenceQuestionId\":null},{\"id\":25,\"content\":\"Thanh Tr\\u00EC\",\"referenceQuestionId\":null},{\"id\":26,\"content\":\"Th\\u01B0\\u1EDDng T\\u00EDn\",\"referenceQuestionId\":null},{\"id\":27,\"content\":\" \\u1EE8ng H\\u00F2a\",\"referenceQuestionId\":null},{\"id\":28,\"content\":\"kh\\u00E1c\",\"referenceQuestionId\":null}]", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Chọn Quận/Huyện", 1, 1.2f, null, false, true, "Bạn sống ở đâu", "DropDown", null, null, null }
                });
        }
    }
}
