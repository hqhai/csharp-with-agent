using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Fsel.Interaction.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSurveyQuestionTableV3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("fcd14833-b79f-4cf4-9ee8-65b041a7daf1"));

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("06b1a0df-eaef-4be9-a9da-9c2633e3a12a"),
                column: "Question",
                value: "Bạn có máy tính để bàn hoặc máy tính xách tay ở nhà để con bạn có thể sử dụng để truy cập khóa học không?");

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("0f9df809-9ae9-4e8c-891d-ec645483f290"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"\\u003C11\",\"referenceQuestionId\":[\"98a97cf1-8562-4828-9cf2-6f5823bf09fe\",\"1fde61af-e1e0-4027-ad21-1176ad212119\"]},{\"id\":2,\"content\":\"11\",\"referenceQuestionId\":[\"73ab87b1-c0ae-40c6-ba37-1cdce350563a\",\"06b1a0df-eaef-4be9-a9da-9c2633e3a12a\",\"83a5b867-c3a1-47b9-91e3-a6fd6086501e\",\"56628f4a-0384-426c-b849-81ab1671afa4\",\"f05b02dc-6393-41dd-a479-cc9c05d1da93\",\"651414ad-cbcf-462a-84b9-daf83880b25a\",\"ef128343-2e29-4735-82f7-9d8d2231dcc2\",\"20675350-89f2-47e3-8e64-b04f2da6290b\",\"98a97cf1-8562-4828-9cf2-6f5823bf09fe\",\"1fde61af-e1e0-4027-ad21-1176ad212119\"]},{\"id\":3,\"content\":\"12\",\"referenceQuestionId\":[\"73ab87b1-c0ae-40c6-ba37-1cdce350563a\",\"06b1a0df-eaef-4be9-a9da-9c2633e3a12a\",\"83a5b867-c3a1-47b9-91e3-a6fd6086501e\",\"56628f4a-0384-426c-b849-81ab1671afa4\",\"f05b02dc-6393-41dd-a479-cc9c05d1da93\",\"651414ad-cbcf-462a-84b9-daf83880b25a\",\"ef128343-2e29-4735-82f7-9d8d2231dcc2\",\"20675350-89f2-47e3-8e64-b04f2da6290b\",\"98a97cf1-8562-4828-9cf2-6f5823bf09fe\",\"1fde61af-e1e0-4027-ad21-1176ad212119\"]},{\"id\":4,\"content\":\"13\",\"referenceQuestionId\":[\"73ab87b1-c0ae-40c6-ba37-1cdce350563a\",\"06b1a0df-eaef-4be9-a9da-9c2633e3a12a\",\"83a5b867-c3a1-47b9-91e3-a6fd6086501e\",\"56628f4a-0384-426c-b849-81ab1671afa4\",\"f05b02dc-6393-41dd-a479-cc9c05d1da93\",\"651414ad-cbcf-462a-84b9-daf83880b25a\",\"ef128343-2e29-4735-82f7-9d8d2231dcc2\",\"20675350-89f2-47e3-8e64-b04f2da6290b\",\"98a97cf1-8562-4828-9cf2-6f5823bf09fe\",\"1fde61af-e1e0-4027-ad21-1176ad212119\"]},{\"id\":5,\"content\":\"14\",\"referenceQuestionId\":[\"73ab87b1-c0ae-40c6-ba37-1cdce350563a\",\"06b1a0df-eaef-4be9-a9da-9c2633e3a12a\",\"83a5b867-c3a1-47b9-91e3-a6fd6086501e\",\"56628f4a-0384-426c-b849-81ab1671afa4\",\"f05b02dc-6393-41dd-a479-cc9c05d1da93\",\"651414ad-cbcf-462a-84b9-daf83880b25a\",\"ef128343-2e29-4735-82f7-9d8d2231dcc2\",\"20675350-89f2-47e3-8e64-b04f2da6290b\",\"98a97cf1-8562-4828-9cf2-6f5823bf09fe\",\"1fde61af-e1e0-4027-ad21-1176ad212119\"]},{\"id\":6,\"content\":\"15\",\"referenceQuestionId\":[\"73ab87b1-c0ae-40c6-ba37-1cdce350563a\",\"06b1a0df-eaef-4be9-a9da-9c2633e3a12a\",\"83a5b867-c3a1-47b9-91e3-a6fd6086501e\",\"56628f4a-0384-426c-b849-81ab1671afa4\",\"f05b02dc-6393-41dd-a479-cc9c05d1da93\",\"651414ad-cbcf-462a-84b9-daf83880b25a\",\"ef128343-2e29-4735-82f7-9d8d2231dcc2\",\"20675350-89f2-47e3-8e64-b04f2da6290b\",\"98a97cf1-8562-4828-9cf2-6f5823bf09fe\",\"1fde61af-e1e0-4027-ad21-1176ad212119\"]},{\"id\":7,\"content\":\"16\",\"referenceQuestionId\":[\"a6f6b5dc-c72a-496c-859c-2fdc9d97f147\",\"06b1a0df-eaef-4be9-a9da-9c2633e3a12a\",\"83a5b867-c3a1-47b9-91e3-a6fd6086501e\",\"56628f4a-0384-426c-b849-81ab1671afa4\",\"f05b02dc-6393-41dd-a479-cc9c05d1da93\",\"bb50bb8b-4e72-4a5b-a058-cdbc7837f4dc\",\"ef128343-2e29-4735-82f7-9d8d2231dcc2\",\"20675350-89f2-47e3-8e64-b04f2da6290b\",\"98a97cf1-8562-4828-9cf2-6f5823bf09fe\",\"1fde61af-e1e0-4027-ad21-1176ad212119\"]},{\"id\":8,\"content\":\"17\",\"referenceQuestionId\":[\"a6f6b5dc-c72a-496c-859c-2fdc9d97f147\",\"06b1a0df-eaef-4be9-a9da-9c2633e3a12a\",\"83a5b867-c3a1-47b9-91e3-a6fd6086501e\",\"56628f4a-0384-426c-b849-81ab1671afa4\",\"f05b02dc-6393-41dd-a479-cc9c05d1da93\",\"bb50bb8b-4e72-4a5b-a058-cdbc7837f4dc\",\"ef128343-2e29-4735-82f7-9d8d2231dcc2\",\"20675350-89f2-47e3-8e64-b04f2da6290b\",\"98a97cf1-8562-4828-9cf2-6f5823bf09fe\",\"1fde61af-e1e0-4027-ad21-1176ad212119\"]},{\"id\":9,\"content\":\"18\",\"referenceQuestionId\":[\"a6f6b5dc-c72a-496c-859c-2fdc9d97f147\",\"06b1a0df-eaef-4be9-a9da-9c2633e3a12a\",\"83a5b867-c3a1-47b9-91e3-a6fd6086501e\",\"56628f4a-0384-426c-b849-81ab1671afa4\",\"f05b02dc-6393-41dd-a479-cc9c05d1da93\",\"bb50bb8b-4e72-4a5b-a058-cdbc7837f4dc\",\"ef128343-2e29-4735-82f7-9d8d2231dcc2\",\"20675350-89f2-47e3-8e64-b04f2da6290b\",\"98a97cf1-8562-4828-9cf2-6f5823bf09fe\",\"1fde61af-e1e0-4027-ad21-1176ad212119\"]},{\"id\":10,\"content\":\"19\\u002B\",\"referenceQuestionId\":[\"98a97cf1-8562-4828-9cf2-6f5823bf09fe\",\"1fde61af-e1e0-4027-ad21-1176ad212119\"]}]");

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("1fde61af-e1e0-4027-ad21-1176ad212119"),
                column: "Question",
                value: "Tại sao bạn muốn đăng ký chương trình học trải nghiệm của FSEL?");

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("20675350-89f2-47e3-8e64-b04f2da6290b"),
                column: "Question",
                value: "Ngày nào sau đây thuận tiện cho bạn tham dự các buổi họp tại trung tâm trong giai đoạn thử nghiệm? (Chọn nhiều phương án)");

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("52548bc5-3536-478c-978b-05f98815bf31"),
                column: "Type",
                value: "MultipleChoiceVertical");

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("56628f4a-0384-426c-b849-81ab1671afa4"),
                column: "Question",
                value: "Chúng tôi sẽ tổ chức 2 buổi on-site (2 tiếng/buổi) tại trung tâm (33 Lạc Trung hoặc 125 Hoàng Ngân) để lấy ý kiến phản hồi của các bạn trong giai đoạn thử nghiệm. Bạn sẵn sàng đưa con mình tham dự những buổi học này ở mức độ nào?");

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("73ab87b1-c0ae-40c6-ba37-1cdce350563a"),
                column: "Question",
                value: "Bạn nhà có thể cam kết hoàn thành khóa học 6 tháng với tốc độ 3 buổi học mỗi tuần (7,5 giờ mỗi tuần) từ tháng 10 năm 2023 đến tháng 3 năm 2024 không?");

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("83a5b867-c3a1-47b9-91e3-a6fd6086501e"),
                column: "Question",
                value: "Con bạn có thể mang máy tính xách tay đến các buổi học tại trung tâm chúng tôi để tham gia khóa học không?");

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("98a97cf1-8562-4828-9cf2-6f5823bf09fe"),
                column: "Question",
                value: "Bạn biết đến chương trình học trải nghiệm của FSEL bằng cách nào?");

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("bb50bb8b-4e72-4a5b-a058-cdbc7837f4dc"),
                column: "Question",
                value: "Ngày nào sau đây thuận tiện cho con bạn tham dự các buổi học trong giai đoạn thử nghiệm? Lưu ý rằng các buổi học sẽ chỉ được tổ chức khoảng một lần mỗi 2 tháng. (Chọn nhiều phương án)");

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("ef128343-2e29-4735-82f7-9d8d2231dcc2"),
                columns: new[] { "AnswerStr", "Question" },
                values: new object[] { "[{\"id\":1,\"content\":\"1 (Kh\\u00F4ng s\\u1EB5n s\\u00E0ng)\",\"referenceQuestionId\":null},{\"id\":2,\"content\":\"2\",\"referenceQuestionId\":null},{\"id\":3,\"content\":\"3\",\"referenceQuestionId\":null},{\"id\":4,\"content\":\"4\",\"referenceQuestionId\":null},{\"id\":5,\"content\":\"5\",\"referenceQuestionId\":null},{\"id\":6,\"content\":\"6\",\"referenceQuestionId\":null},{\"id\":7,\"content\":\"7\",\"referenceQuestionId\":null},{\"id\":8,\"content\":\"8\",\"referenceQuestionId\":null},{\"id\":9,\"content\":\"9\",\"referenceQuestionId\":null},{\"id\":10,\"content\":\"10 (R\\u1EA5t s\\u1EB5n s\\u00E0ng)\",\"referenceQuestionId\":null}]", "Bạn có sẵn sàng tham dự các buổi họp tại trung tâm để tham gia các cuộc phỏng vấn và các buổi họp nhóm tập trung cho phụ huynh không?" });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("f05b02dc-6393-41dd-a479-cc9c05d1da93"),
                columns: new[] { "Question", "Type" },
                values: new object[] { "Bạn mong muốn các buổi họp được tổ chức tại trung tâm nào hơn?", "MultipleChoiceVertical" });

            migrationBuilder.InsertData(
                table: "SurveyQuestions",
                columns: new[] { "Id", "AnswerStr", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "Description", "DisplayLevel", "DisplayOrder", "Icon", "IsDeleted", "IsPilot", "Question", "Type", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[,]
                {
                    { new Guid("651414ad-cbcf-462a-84b9-daf83880b25a"), "[{\"id\":1,\"content\":\"C\\u00E1c bu\\u1ED5i t\\u1ED1i th\\u1EE9 Hai\",\"referenceQuestionId\":null},{\"id\":2,\"content\":\"C\\u00E1c bu\\u1ED5i t\\u1ED1i th\\u1EE9 Ba\",\"referenceQuestionId\":null},{\"id\":3,\"content\":\"C\\u00E1c bu\\u1ED5i t\\u1ED1i th\\u1EE9 T\\u01B0\",\"referenceQuestionId\":null},{\"id\":4,\"content\":\"C\\u00E1c bu\\u1ED5i t\\u1ED1i th\\u1EE9 N\\u0103m\",\"referenceQuestionId\":null},{\"id\":5,\"content\":\"C\\u00E1c bu\\u1ED5i t\\u1ED1i th\\u1EE9 S\\u00E1u\",\"referenceQuestionId\":null},{\"id\":6,\"content\":\"C\\u00E1c bu\\u1ED5i s\\u00E1ng th\\u1EE9 B\\u1EA3y\",\"referenceQuestionId\":null},{\"id\":7,\"content\":\"C\\u00E1c bu\\u1ED5i chi\\u1EC1u th\\u1EE9 B\\u1EA3y\",\"referenceQuestionId\":null},{\"id\":8,\"content\":\"C\\u00E1c bu\\u1ED5i t\\u1ED1i th\\u1EE9 B\\u1EA3y\",\"referenceQuestionId\":null},{\"id\":9,\"content\":\"C\\u00E1c bu\\u1ED5i s\\u00E1ng ch\\u1EE7 nh\\u1EADt\",\"referenceQuestionId\":null},{\"id\":10,\"content\":\"C\\u00E1c bu\\u1ED5i chi\\u1EC1u ch\\u1EE7 nh\\u1EADt\",\"referenceQuestionId\":null},{\"id\":11,\"content\":\"C\\u00E1c bu\\u1ED5i t\\u1ED1i ch\\u1EE7 nh\\u1EADt\",\"referenceQuestionId\":null}]", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "addd", 2, 9f, null, false, true, "Ngày nào sau đây thuận tiện cho con bạn tham dự các buổi học trong giai đoạn thử nghiệm? Lưu ý rằng các buổi học sẽ chỉ được tổ chức khoảng một lần mỗi 3 tháng. (Chọn nhiều phương án)", "CheckList", null, null, null },
                    { new Guid("a6f6b5dc-c72a-496c-859c-2fdc9d97f147"), "[{\"id\":1,\"content\":\"C\\u00F3\",\"referenceQuestionId\":null},{\"id\":2,\"content\":\"Kh\\u00F4ng\",\"referenceQuestionId\":null}]", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "addd", 2, 4f, null, false, true, "Bạn nhà có thể cam kết hoàn thành khóa học 4 tháng với tốc độ 2 buổi học mỗi tuần (5 giờ mỗi tuần) từ tháng 11 năm 2023 đến tháng 2 năm 2024 không?", "MultipleChoiceVertical", null, null, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("651414ad-cbcf-462a-84b9-daf83880b25a"));

            migrationBuilder.DeleteData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("a6f6b5dc-c72a-496c-859c-2fdc9d97f147"));

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("06b1a0df-eaef-4be9-a9da-9c2633e3a12a"),
                column: "Question",
                value: "Bạn có PC hoặc Laptop ở nhà để con bạn có thể sử dụng để truy cập khóa học không?");

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("0f9df809-9ae9-4e8c-891d-ec645483f290"),
                column: "AnswerStr",
                value: "[{\"id\":1,\"content\":\"\\u003C11\",\"referenceQuestionId\":[\"98a97cf1-8562-4828-9cf2-6f5823bf09fe\",\"1fde61af-e1e0-4027-ad21-1176ad212119\"]},{\"id\":2,\"content\":\"11\",\"referenceQuestionId\":[\"73ab87b1-c0ae-40c6-ba37-1cdce350563a\",\"06b1a0df-eaef-4be9-a9da-9c2633e3a12a\",\"56628f4a-0384-426c-b849-81ab1671afa4\",\"f05b02dc-6393-41dd-a479-cc9c05d1da93\",\"bb50bb8b-4e72-4a5b-a058-cdbc7837f4dc\",\"ef128343-2e29-4735-82f7-9d8d2231dcc2\",\"20675350-89f2-47e3-8e64-b04f2da6290b\",\"98a97cf1-8562-4828-9cf2-6f5823bf09fe\",\"1fde61af-e1e0-4027-ad21-1176ad212119\"]},{\"id\":3,\"content\":\"12\",\"referenceQuestionId\":[\"73ab87b1-c0ae-40c6-ba37-1cdce350563a\",\"06b1a0df-eaef-4be9-a9da-9c2633e3a12a\",\"56628f4a-0384-426c-b849-81ab1671afa4\",\"f05b02dc-6393-41dd-a479-cc9c05d1da93\",\"bb50bb8b-4e72-4a5b-a058-cdbc7837f4dc\",\"ef128343-2e29-4735-82f7-9d8d2231dcc2\",\"20675350-89f2-47e3-8e64-b04f2da6290b\",\"98a97cf1-8562-4828-9cf2-6f5823bf09fe\",\"1fde61af-e1e0-4027-ad21-1176ad212119\"]},{\"id\":4,\"content\":\"13\",\"referenceQuestionId\":[\"73ab87b1-c0ae-40c6-ba37-1cdce350563a\",\"06b1a0df-eaef-4be9-a9da-9c2633e3a12a\",\"56628f4a-0384-426c-b849-81ab1671afa4\",\"f05b02dc-6393-41dd-a479-cc9c05d1da93\",\"bb50bb8b-4e72-4a5b-a058-cdbc7837f4dc\",\"ef128343-2e29-4735-82f7-9d8d2231dcc2\",\"20675350-89f2-47e3-8e64-b04f2da6290b\",\"98a97cf1-8562-4828-9cf2-6f5823bf09fe\",\"1fde61af-e1e0-4027-ad21-1176ad212119\"]},{\"id\":5,\"content\":\"14\",\"referenceQuestionId\":[\"73ab87b1-c0ae-40c6-ba37-1cdce350563a\",\"06b1a0df-eaef-4be9-a9da-9c2633e3a12a\",\"56628f4a-0384-426c-b849-81ab1671afa4\",\"f05b02dc-6393-41dd-a479-cc9c05d1da93\",\"bb50bb8b-4e72-4a5b-a058-cdbc7837f4dc\",\"ef128343-2e29-4735-82f7-9d8d2231dcc2\",\"20675350-89f2-47e3-8e64-b04f2da6290b\",\"98a97cf1-8562-4828-9cf2-6f5823bf09fe\",\"1fde61af-e1e0-4027-ad21-1176ad212119\"]},{\"id\":6,\"content\":\"15\",\"referenceQuestionId\":[\"73ab87b1-c0ae-40c6-ba37-1cdce350563a\",\"06b1a0df-eaef-4be9-a9da-9c2633e3a12a\",\"56628f4a-0384-426c-b849-81ab1671afa4\",\"f05b02dc-6393-41dd-a479-cc9c05d1da93\",\"bb50bb8b-4e72-4a5b-a058-cdbc7837f4dc\",\"ef128343-2e29-4735-82f7-9d8d2231dcc2\",\"20675350-89f2-47e3-8e64-b04f2da6290b\",\"98a97cf1-8562-4828-9cf2-6f5823bf09fe\",\"1fde61af-e1e0-4027-ad21-1176ad212119\"]},{\"id\":7,\"content\":\"16\",\"referenceQuestionId\":[\"fcd14833-b79f-4cf4-9ee8-65b041a7daf1\",\"06b1a0df-eaef-4be9-a9da-9c2633e3a12a\",\"56628f4a-0384-426c-b849-81ab1671afa4\",\"f05b02dc-6393-41dd-a479-cc9c05d1da93\",\"bb50bb8b-4e72-4a5b-a058-cdbc7837f4dc\",\"ef128343-2e29-4735-82f7-9d8d2231dcc2\",\"20675350-89f2-47e3-8e64-b04f2da6290b\",\"98a97cf1-8562-4828-9cf2-6f5823bf09fe\",\"1fde61af-e1e0-4027-ad21-1176ad212119\"]},{\"id\":8,\"content\":\"17\",\"referenceQuestionId\":[\"fcd14833-b79f-4cf4-9ee8-65b041a7daf1\",\"06b1a0df-eaef-4be9-a9da-9c2633e3a12a\",\"56628f4a-0384-426c-b849-81ab1671afa4\",\"f05b02dc-6393-41dd-a479-cc9c05d1da93\",\"bb50bb8b-4e72-4a5b-a058-cdbc7837f4dc\",\"ef128343-2e29-4735-82f7-9d8d2231dcc2\",\"20675350-89f2-47e3-8e64-b04f2da6290b\",\"98a97cf1-8562-4828-9cf2-6f5823bf09fe\",\"1fde61af-e1e0-4027-ad21-1176ad212119\"]},{\"id\":9,\"content\":\"18\",\"referenceQuestionId\":[\"fcd14833-b79f-4cf4-9ee8-65b041a7daf1\",\"06b1a0df-eaef-4be9-a9da-9c2633e3a12a\",\"56628f4a-0384-426c-b849-81ab1671afa4\",\"f05b02dc-6393-41dd-a479-cc9c05d1da93\",\"bb50bb8b-4e72-4a5b-a058-cdbc7837f4dc\",\"ef128343-2e29-4735-82f7-9d8d2231dcc2\",\"20675350-89f2-47e3-8e64-b04f2da6290b\",\"98a97cf1-8562-4828-9cf2-6f5823bf09fe\",\"1fde61af-e1e0-4027-ad21-1176ad212119\"]},{\"id\":10,\"content\":\"19\\u002B\",\"referenceQuestionId\":[\"98a97cf1-8562-4828-9cf2-6f5823bf09fe\",\"1fde61af-e1e0-4027-ad21-1176ad212119\"]}]");

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("1fde61af-e1e0-4027-ad21-1176ad212119"),
                column: "Question",
                value: "Tại sao bạn muốn đăng ký FSEL Pilot?");

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("20675350-89f2-47e3-8e64-b04f2da6290b"),
                column: "Question",
                value: "Ngày nào sau đây thuận tiện cho con bạn tham dự các buổi học tại chỗ trong thời gian thí điểm? (Chọn nhiều phương án)");

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("52548bc5-3536-478c-978b-05f98815bf31"),
                column: "Type",
                value: "AgeGender");

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("56628f4a-0384-426c-b849-81ab1671afa4"),
                column: "Question",
                value: "Chúng tôi sẽ tổ chức 4 buổi on-site (2 tiếng/buổi) tại trung tâm (33 Lạc Trung hoặc 125 Hoàng Ngân) để lấy ý kiến ​​phản hồi của các bạn trong thời gian thí điểm. Bạn sẵn sàng đưa con mình tham dự những buổi học này ở mức độ nào?");

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("73ab87b1-c0ae-40c6-ba37-1cdce350563a"),
                column: "Question",
                value: "Con bạn có thể cam kết hoàn thành khóa học thí điểm 6 tháng với tốc độ 3 buổi học mỗi tuần (7,5 giờ mỗi tuần) từ tháng 10 năm 2023 đến tháng 3 năm 2024 không?");

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("83a5b867-c3a1-47b9-91e3-a6fd6086501e"),
                column: "Question",
                value: "Con bạn có thể mượn máy tính xách tay và mang đến các buổi học thí điểm tại trung tâm chúng tôi để tham gia khóa học không?");

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("98a97cf1-8562-4828-9cf2-6f5823bf09fe"),
                column: "Question",
                value: "Bạn Thích vị trí nào hơn cho các buổi họp tại chỗ?");

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("bb50bb8b-4e72-4a5b-a058-cdbc7837f4dc"),
                column: "Question",
                value: "Ngày nào sau đây thuận tiện cho con bạn tham dự các buổi học tại chỗ trong thời gian thí điểm? Lưu ý rằng các buổi học tại chỗ sẽ chỉ được tổ chức khoảng một lần mỗi tháng.(Chọn nhiều phương án)");

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("ef128343-2e29-4735-82f7-9d8d2231dcc2"),
                columns: new[] { "AnswerStr", "Question" },
                values: new object[] { "[{\"id\":1,\"content\":\"1 (Kh\\u00F4ng s\\u1EB5n l\\u00F2ng ch\\u00FAt n\\u00E0o)\",\"referenceQuestionId\":null},{\"id\":2,\"content\":\"2\",\"referenceQuestionId\":null},{\"id\":3,\"content\":\"3\",\"referenceQuestionId\":null},{\"id\":4,\"content\":\"4\",\"referenceQuestionId\":null},{\"id\":5,\"content\":\"5\",\"referenceQuestionId\":null},{\"id\":6,\"content\":\"6\",\"referenceQuestionId\":null},{\"id\":7,\"content\":\"7\",\"referenceQuestionId\":null},{\"id\":8,\"content\":\"8\",\"referenceQuestionId\":null},{\"id\":9,\"content\":\"9\",\"referenceQuestionId\":null},{\"id\":10,\"content\":\"10 (R\\u1EA5t s\\u1EB5n s\\u00E0ng)\",\"referenceQuestionId\":null}]", "Bạn có sẵn lòng tham dự các buổi họp tại chỗ để tham gia các cuộc phóng vấn phụ huynh và các buổi họp nhóm tập trung không?" });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("f05b02dc-6393-41dd-a479-cc9c05d1da93"),
                columns: new[] { "Question", "Type" },
                values: new object[] { "Bạn thích vị trí nào hơn cho các buổi họp tại chỗ?", "ShortAnswer" });

            migrationBuilder.InsertData(
                table: "SurveyQuestions",
                columns: new[] { "Id", "AnswerStr", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "Description", "DisplayLevel", "DisplayOrder", "Icon", "IsDeleted", "IsPilot", "Question", "Type", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[] { new Guid("fcd14833-b79f-4cf4-9ee8-65b041a7daf1"), "[{\"id\":1,\"content\":\"C\\u00F3\",\"referenceQuestionId\":null},{\"id\":2,\"content\":\"Kh\\u00F4ng\",\"referenceQuestionId\":null}]", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "addd", 2, 4f, null, false, true, "Con bạn có thể cam kết hoàn thành khóa học thí điểm 4 tháng với tốc độ 3 buổi học mỗi tuần (5 giờ mỗi tuần) từ tháng 11 năm 2023 đến tháng 2 năm 2024 không?", "MultipleChoiceVertical", null, null, null });
        }
    }
}
