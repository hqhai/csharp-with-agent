using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Interaction.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSurveyQuestionV5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("b695a761-d0fa-4162-97c3-68403e9a8326"),
                column: "Question",
                value: "Giới tính của bạn");

            migrationBuilder.UpdateData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("e343b238-f2fc-418a-a171-6cce3d90d2a1"),
                column: "Question",
                value: "Your Gender");

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("2be9a620-143d-41f6-815b-2038c21a7b23"),
                column: "Question",
                value: "Giới tính của bạn");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("b695a761-d0fa-4162-97c3-68403e9a8326"),
                column: "Question",
                value: "Xác định độ tuổi và giới tính");

            migrationBuilder.UpdateData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("e343b238-f2fc-418a-a171-6cce3d90d2a1"),
                column: "Question",
                value: "age and gender");

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("2be9a620-143d-41f6-815b-2038c21a7b23"),
                column: "Question",
                value: "Xác định độ tuổi và giới tính");
        }
    }
}
