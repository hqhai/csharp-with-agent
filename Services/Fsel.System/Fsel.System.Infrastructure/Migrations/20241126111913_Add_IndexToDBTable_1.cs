using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.System.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_IndexToDBTable_1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_StudentTechies_CreatedUserId_IsDeleted_TechieActionId",
                table: "StudentTechies",
                columns: new[] { "CreatedUserId", "IsDeleted", "TechieActionId" });

            migrationBuilder.CreateIndex(
                name: "IX_QuestBoardStudents_IsDeleted_QuestBoardId_StudentId_CreatedDate",
                table: "QuestBoardStudents",
                columns: new[] { "IsDeleted", "QuestBoardId", "StudentId", "CreatedDate" });

            migrationBuilder.CreateIndex(
                name: "IX_FeatureAccessTimes_CreatedUserId_IsDeleted_CourseId_UnitId_ObjectId",
                table: "FeatureAccessTimes",
                columns: new[] { "CreatedUserId", "IsDeleted", "CourseId", "UnitId", "ObjectId" });

            migrationBuilder.CreateIndex(
                name: "IX_FeatureAccessTimes_CreatedUserId_IsDeleted_EnumFeature",
                table: "FeatureAccessTimes",
                columns: new[] { "CreatedUserId", "IsDeleted", "EnumFeature" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StudentTechies_CreatedUserId_IsDeleted_TechieActionId",
                table: "StudentTechies");

            migrationBuilder.DropIndex(
                name: "IX_QuestBoardStudents_IsDeleted_QuestBoardId_StudentId_CreatedDate",
                table: "QuestBoardStudents");

            migrationBuilder.DropIndex(
                name: "IX_FeatureAccessTimes_CreatedUserId_IsDeleted_CourseId_UnitId_ObjectId",
                table: "FeatureAccessTimes");

            migrationBuilder.DropIndex(
                name: "IX_FeatureAccessTimes_CreatedUserId_IsDeleted_EnumFeature",
                table: "FeatureAccessTimes");
        }
    }
}
