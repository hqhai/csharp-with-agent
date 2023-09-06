using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Fsel.Interaction.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSurveyQuestionTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                column: "IsPilot",
                value: false);

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("721d8bb9-cdbe-42e7-aa16-35a1915c1123"),
                column: "IsPilot",
                value: false);

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("763d8bb9-cdbe-42e7-aa16-35a1915c3512"),
                column: "IsPilot",
                value: false);

            migrationBuilder.InsertData(
                table: "SurveyQuestions",
                columns: new[] { "Id", "AnswerStr", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "Description", "DisplayOrder", "Icon", "IsDeleted", "IsPilot", "Question", "Type", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[,]
                {
                    { new Guid("06b1a0df-eaef-4be9-a9da-9c2633e3a12a"), "[{\"id\":1,\"content\":\"Laptop\"},{\"id\":2,\"content\":\"PC\"},{\"id\":3,\"content\":\"Both\"},{\"id\":4,\"content\":\"No\"}]", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "addd", 13, null, false, true, "Bạn có PC hoặc Laptop ở nhà để con bạn có thể sử dụng để truy cập khóa học không?", "MultipleChoiceVertical", null, null, null },
                    { new Guid("0f9df809-9ae9-4e8c-891d-ec645483f290"), "[{\"id\":1,\"age\":\"\\u003C11\"},{\"id\":2,\"age\":\"11\"},{\"id\":3,\"age\":\"12\"},{\"id\":4,\"age\":\"13\"},{\"id\":5,\"age\":\"14\"},{\"id\":6,\"age\":\"15\"},{\"id\":7,\"age\":\"16\"},{\"id\":8,\"age\":\"17\"},{\"id\":9,\"age\":\"18\"},{\"id\":10,\"age\":\"19\\u002B\"}]", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "addd", 9, null, false, true, "Xác định độ tuổi và giới tính", "DropDown", null, null, null },
                    { new Guid("1fde61af-e1e0-4027-ad21-1176ad212119"), "null", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "addd", 18, null, false, true, "Tại sao bạn muốn đăng ký FSEL Pilot?", "ShortAnswer", null, null, null },
                    { new Guid("20675350-89f2-47e3-8e64-b04f2da6290b"), "[{\"id\":1,\"content\":\"Monday evenings\"},{\"id\":2,\"content\":\"Tuesday evenings\"},{\"id\":3,\"content\":\"Wednesday evenings\"},{\"id\":4,\"content\":\"Thursday evenings\"},{\"id\":5,\"content\":\"Friday evenings\"},{\"id\":6,\"content\":\"Saturday mornings\"},{\"id\":7,\"content\":\"Saturday afternoons\"},{\"id\":8,\"content\":\"Saturday evenings\"},{\"id\":9,\"content\":\"Sunday mornings\"},{\"id\":10,\"content\":\"Sunday afternoons\"},{\"id\":11,\"content\":\"Sunday evenings\"}]", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "addd", 16, null, false, true, "Ngày nào sau đây thuận tiện cho con bạn tham dự các buổi học tại chỗ trong thời gian thí điểm? (Chọn nhiều phương án)", "CheckList", null, null, null },
                    { new Guid("52548bc5-3536-478c-978b-05f98815bf31"), "[{\"id\":1,\"gender\":\"Male\"},{\"id\":2,\"gender\":\"Female\"},{\"id\":3,\"gender\":\"Other\"}]", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "addd", 10, null, false, true, "Xác định độ tuổi và giới tính", "AgeGender", null, null, null },
                    { new Guid("5317ae1a-46db-4f25-9625-f9c28bd3c688"), "null", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "addd", 17, null, false, true, "Bạn biết đến FSEL Pilot bằng cách nào?", "ShortAnswer", null, null, null },
                    { new Guid("56628f4a-0384-426c-b849-81ab1671afa4"), "[{\"id\":1,\"content\":\"1 (Not willing at all)\"},{\"id\":2,\"content\":\"2\"},{\"id\":3,\"content\":\"3\"},{\"id\":4,\"content\":\"4\"},{\"id\":5,\"content\":\"5\"},{\"id\":6,\"content\":\"6\"},{\"id\":7,\"content\":\"7\"},{\"id\":8,\"content\":\"8\"},{\"id\":9,\"content\":\"9\"},{\"id\":10,\"content\":\"10 (Very Willing)\"}]", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "addd", 15, null, false, true, "Chúng tôi sẽ tổ chức 4 buổi on-site (2 tiếng/buổi) tại trung tâm (33 Lạc Trung hoặc 125 Hoàng Ngân) để lấy ý kiến ​​phản hồi của các bạn trong thời gian thí điểm. Bạn sẵn sàng đưa con mình tham dự những buổi học này ở mức độ nào?", "MultipleChoiceHorizontal", null, null, null },
                    { new Guid("73ab87b1-c0ae-40c6-ba37-1cdce350563a"), "[{\"id\":1,\"content\":\"Yes\"},{\"id\":2,\"content\":\"No\"}]", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "addd", 11, null, false, true, "Con bạn có thể cam kết hoàn thành khóa học thí điểm 6 tháng với tốc độ 3 buổi học mỗi tuần (7,5 giờ mỗi tuần) từ tháng 10 năm 2023 đến tháng 3 năm 2024 không?", "MultipleChoiceVertical", null, null, null },
                    { new Guid("83a5b867-c3a1-47b9-91e3-a6fd6086501e"), "[{\"id\":1,\"content\":\"Yes\"},{\"id\":2,\"content\":\"No\"}]", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "addd", 14, null, false, true, "Con bạn có thể mượn máy tính xách tay và mang đến các buổi học thí điểm tại trung tâm chúng tôi để tham gia khóa học không?", "MultipleChoiceVertical", null, null, null },
                    { new Guid("9619b7d3-b2ab-4462-9044-b443ab79942a"), "null", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "addd", 7, null, false, true, "Số điện thoại của bạn là gì", "ShortAnswer", null, null, null },
                    { new Guid("cedef58c-7d94-4efc-a030-0216b3034bef"), "null", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "addd", 8, null, false, true, "Họ tên đầy đủ của trẻ nhà bạn là gì", "ShortAnswer", null, null, null },
                    { new Guid("ee0e74f5-83ae-44dd-a7d0-0f7b650884f8"), "null", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Câu trả lời của bạn", 6, null, false, true, "Họ tên đầy đủ của bạn là gì", "ShortAnswer", null, null, null },
                    { new Guid("f959d3d1-cab3-4fc6-8341-cb44bcbd3e30"), "[{\"id\":1,\"district\":\"Ba \\u0110\\u00ECnh\"},{\"id\":2,\"district\":\"Ho\\u00E0n Ki\\u1EBFm\"},{\"id\":3,\"district\":\"T\\u00E2y H\\u1ED3\"},{\"id\":4,\"district\":\"Long Bi\\u00EAn\"},{\"id\":5,\"district\":\"C\\u1EA7u Gi\\u1EA5y\"},{\"id\":6,\"district\":\"\\u0110\\u1ED1ng \\u0110a\"},{\"id\":7,\"district\":\"Hai B\\u00E0 Tr\\u01B0ng\"},{\"id\":8,\"district\":\"Ho\\u00E0ng Mai\"},{\"id\":9,\"district\":\"Thanh Xu\\u00E2n\"},{\"id\":10,\"district\":\"H\\u00E0 \\u0110\\u00F4ng\"},{\"id\":11,\"district\":\"B\\u1EAFc T\\u1EEB Li\\u00EAm\"},{\"id\":12,\"district\":\"Nam T\\u1EEB Li\\u00EAm\"},{\"id\":13,\"district\":\"Ba V\\u00EC\"},{\"id\":14,\"district\":\"Ch\\u01B0\\u01A1ng M\\u1EF9\"},{\"id\":15,\"district\":\"\\u0110an Ph\\u01B0\\u1EE3ng\"},{\"id\":16,\"district\":\"\\u0110\\u00F4ng Anh\"},{\"id\":17,\"district\":\"Gia L\\u00E2m\"},{\"id\":18,\"district\":\"Ho\\u00E0i \\u0110\\u1EE9c\"},{\"id\":19,\"district\":\"M\\u00EA Linh\"},{\"id\":20,\"district\":\"Ph\\u00FA Xuy\\u00EAn\"},{\"id\":21,\"district\":\"Ph\\u00FAc Th\\u1ECD\"},{\"id\":22,\"district\":\"Qu\\u1ED1c Oai\"},{\"id\":23,\"district\":\"Th\\u1EA1ch Th\\u1EA5t\"},{\"id\":24,\"district\":\"Thanh Oai\"},{\"id\":25,\"district\":\"Thanh Tr\\u00EC\"},{\"id\":26,\"district\":\"Th\\u01B0\\u1EDDng T\\u00EDn\"},{\"id\":27,\"district\":\" \\u1EE8ng H\\u00F2a\"},{\"id\":28,\"district\":\"kh\\u00E1c\"}]", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "addd", 7, null, false, true, "Bạn Sống ở đâu", "DropDown", null, null, null },
                    { new Guid("fcd14833-b79f-4cf4-9ee8-65b041a7daf1"), "[{\"id\":1,\"content\":\"Yes\"},{\"id\":2,\"content\":\"No\"}]", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "addd", 12, null, false, true, "Con bạn có thể cam kết hoàn thành khóa học thí điểm 4 tháng với tốc độ 3 buổi học mỗi tuần (5 giờ mỗi tuần) từ tháng 11 năm 2023 đến tháng 2 năm 2024 không?", "MultipleChoiceVertical", null, null, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
                keyValue: new Guid("5317ae1a-46db-4f25-9625-f9c28bd3c688"));

            migrationBuilder.DeleteData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("56628f4a-0384-426c-b849-81ab1671afa4"));

            migrationBuilder.DeleteData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("73ab87b1-c0ae-40c6-ba37-1cdce350563a"));

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
                keyValue: new Guid("cedef58c-7d94-4efc-a030-0216b3034bef"));

            migrationBuilder.DeleteData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("ee0e74f5-83ae-44dd-a7d0-0f7b650884f8"));

            migrationBuilder.DeleteData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("f959d3d1-cab3-4fc6-8341-cb44bcbd3e30"));

            migrationBuilder.DeleteData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("fcd14833-b79f-4cf4-9ee8-65b041a7daf1"));

            migrationBuilder.DropColumn(
                name: "IsPilot",
                table: "SurveyQuestions");
        }
    }
}
