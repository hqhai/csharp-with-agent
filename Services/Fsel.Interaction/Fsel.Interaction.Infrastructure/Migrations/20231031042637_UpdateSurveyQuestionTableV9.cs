using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Interaction.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSurveyQuestionTableV9 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("56628f4a-0384-426c-b849-81ab1671afa4"),
                column: "Question",
                value: "Chúng tôi sẽ tổ chức 2 buổi gặp mặt trực tiếp (2 tiếng/buổi) tại trung tâm (33 Lạc Trung hoặc 125 Hoàng Ngân) để lấy ý kiến phản hồi của các bạn trong giai đoạn thử nghiệm. Bạn sẵn sàng đưa con mình tham dự những buổi học này ở mức độ nào?");

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("73ab87b1-c0ae-40c6-ba37-1cdce350563a"),
                column: "Question",
                value: "Bạn nhà có thể cam kết hoàn thành khóa học 6 tháng với tốc độ 3 buổi học mỗi tuần (7,5 giờ mỗi tuần) từ tháng 11 năm 2023 đến tháng 4 năm 2024 không?");

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("ee0e74f5-83ae-44dd-a7d0-0f7b650884f8"),
                column: "Question",
                value: "Họ tên");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
                keyValue: new Guid("ee0e74f5-83ae-44dd-a7d0-0f7b650884f8"),
                column: "Question",
                value: "Họ tên đầy đủ của bạn là gì");
        }
    }
}
