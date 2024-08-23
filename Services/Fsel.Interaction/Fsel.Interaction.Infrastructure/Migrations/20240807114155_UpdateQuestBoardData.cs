using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Interaction.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateQuestBoardData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("365375de-db86-4167-bbe8-e5a3ca2c154e"),
                column: "Question",
                value: "Thông tin cá nhân và Trường học");

            migrationBuilder.UpdateData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("4393a6a5-ff24-4d2d-b468-4b2c17ae0063"),
                column: "Question",
                value: "Personal Infomation And School");

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("713d8bb9-cdbe-42e7-aa16-35a1915c3532"),
                columns: new[] { "Description", "Question" },
                values: new object[] { "Thông tin cá nhân", "Thông tin cá nhân" });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("b223125a-a4e1-4e10-b4dd-cfcd747d74c5"),
                column: "Question",
                value: "Thông tin cá nhân và Trường học");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("365375de-db86-4167-bbe8-e5a3ca2c154e"),
                column: "Question",
                value: "Trường học của bạn");

            migrationBuilder.UpdateData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("4393a6a5-ff24-4d2d-b468-4b2c17ae0063"),
                column: "Question",
                value: "Your school");

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("713d8bb9-cdbe-42e7-aa16-35a1915c3532"),
                columns: new[] { "Description", "Question" },
                values: new object[] { "addd", "Vị trí của bạn" });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("b223125a-a4e1-4e10-b4dd-cfcd747d74c5"),
                column: "Question",
                value: "Trường học của bạn");
        }
    }
}
