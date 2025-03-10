using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Interaction.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_IndexToDBTable_1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_CustomerSurveys_IsDeleted_UserId",
                table: "CustomerSurveys",
                columns: new[] { "IsDeleted", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerSurveyGroups_IsDeleted_UserId_SurveyFormType",
                table: "CustomerSurveyGroups",
                columns: new[] { "IsDeleted", "UserId", "SurveyFormType" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CustomerSurveys_IsDeleted_UserId",
                table: "CustomerSurveys");

            migrationBuilder.DropIndex(
                name: "IX_CustomerSurveyGroups_IsDeleted_UserId_SurveyFormType",
                table: "CustomerSurveyGroups");
        }
    }
}
