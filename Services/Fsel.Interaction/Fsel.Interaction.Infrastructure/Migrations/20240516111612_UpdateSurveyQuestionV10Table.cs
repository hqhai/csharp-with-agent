using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Interaction.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSurveyQuestionV10Table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("d9343200-7b1b-475c-8330-a8d3e1a09be4"),
                column: "Question",
                value: "Your location?");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "SurveyQuestionTranslations",
                keyColumn: "Id",
                keyValue: new Guid("d9343200-7b1b-475c-8330-a8d3e1a09be4"),
                column: "Question",
                value: "Your position?");
        }
    }
}
