using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Interaction.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_IndexToDBTable_2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_CustomerSurveys_IsDeleted_UserId_SurveyQuestionId",
                table: "CustomerSurveys",
                columns: new[] { "IsDeleted", "UserId", "SurveyQuestionId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CustomerSurveys_IsDeleted_UserId_SurveyQuestionId",
                table: "CustomerSurveys");
        }
    }
}
