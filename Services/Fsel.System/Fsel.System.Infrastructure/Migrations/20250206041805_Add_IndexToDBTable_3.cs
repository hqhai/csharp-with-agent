using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.System.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_IndexToDBTable_3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_UserConfigs_IsDeleted_UserId_IsViewNewFeature",
                table: "UserConfigs",
                columns: new[] { "IsDeleted", "UserId", "IsViewNewFeature" });

            migrationBuilder.CreateIndex(
                name: "IX_TokenHistories_IsDeleted_UserId_CreatedDate",
                table: "TokenHistories",
                columns: new[] { "IsDeleted", "UserId", "CreatedDate" });

            migrationBuilder.CreateIndex(
                name: "IX_TokenHistories_IsDeleted_UserId_Type",
                table: "TokenHistories",
                columns: new[] { "IsDeleted", "UserId", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_QuestBoardOverallStudents_IsDeleted_QuestBoardOverallId_StudentId_CreatedDate",
                table: "QuestBoardOverallStudents",
                columns: new[] { "IsDeleted", "QuestBoardOverallId", "StudentId", "CreatedDate" });

            migrationBuilder.CreateIndex(
                name: "IX_QuestBoardOverallStudents_IsDeleted_QuestBoardOverallId_StudentId_Status_CurrentValue",
                table: "QuestBoardOverallStudents",
                columns: new[] { "IsDeleted", "QuestBoardOverallId", "StudentId", "Status", "CurrentValue" });

            migrationBuilder.CreateIndex(
                name: "IX_LuckyTickets_IsDeleted_LessonResultId_StudentId",
                table: "LuckyTickets",
                columns: new[] { "IsDeleted", "LessonResultId", "StudentId" });

            migrationBuilder.CreateIndex(
                name: "IX_FselRatings_CreatedUserId_IsDeleted",
                table: "FselRatings",
                columns: new[] { "CreatedUserId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_FeatureAccessTimes_IsDeleted_CreatedUserId",
                table: "FeatureAccessTimes",
                columns: new[] { "IsDeleted", "CreatedUserId" })
                .Annotation("SqlServer:Include", new[] { "AccessTime", "LastVisited" });

            migrationBuilder.CreateIndex(
                name: "IX_BannerStudents_IsDeleted_StudentId_BannerId",
                table: "BannerStudents",
                columns: new[] { "IsDeleted", "StudentId", "BannerId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserConfigs_IsDeleted_UserId_IsViewNewFeature",
                table: "UserConfigs");

            migrationBuilder.DropIndex(
                name: "IX_TokenHistories_IsDeleted_UserId_CreatedDate",
                table: "TokenHistories");

            migrationBuilder.DropIndex(
                name: "IX_TokenHistories_IsDeleted_UserId_Type",
                table: "TokenHistories");

            migrationBuilder.DropIndex(
                name: "IX_QuestBoardOverallStudents_IsDeleted_QuestBoardOverallId_StudentId_CreatedDate",
                table: "QuestBoardOverallStudents");

            migrationBuilder.DropIndex(
                name: "IX_QuestBoardOverallStudents_IsDeleted_QuestBoardOverallId_StudentId_Status_CurrentValue",
                table: "QuestBoardOverallStudents");

            migrationBuilder.DropIndex(
                name: "IX_LuckyTickets_IsDeleted_LessonResultId_StudentId",
                table: "LuckyTickets");

            migrationBuilder.DropIndex(
                name: "IX_FselRatings_CreatedUserId_IsDeleted",
                table: "FselRatings");

            migrationBuilder.DropIndex(
                name: "IX_FeatureAccessTimes_IsDeleted_CreatedUserId",
                table: "FeatureAccessTimes");

            migrationBuilder.DropIndex(
                name: "IX_BannerStudents_IsDeleted_StudentId_BannerId",
                table: "BannerStudents");
        }
    }
}
