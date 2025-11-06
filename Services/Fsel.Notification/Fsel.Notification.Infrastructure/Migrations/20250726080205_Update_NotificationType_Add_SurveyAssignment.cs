using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Fsel.Notification.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_NotificationType_Add_SurveyAssignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "NotificationTypes",
                columns: new[] { "Id", "Content", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "Icon", "IsDeleted", "Priority", "TemplateLink", "TemplateMessage", "Type", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[] { new Guid("edc32213-8546-4e0b-ab27-b909fb03c87e"), "SurveyAssignment", new DateTime(2025, 7, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, 1, "/survey", "📢 Bạn có một bài khảo sát mới! Hoàn thành bài khảo sát ngay để giúp chúng tôi hiểu hơn về bạn và nhận xu FSEL! 🎯", "LinkPage", null, null, null });

            migrationBuilder.InsertData(
                table: "NotificationTypeTranslations",
                columns: new[] { "Id", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "IsDeleted", "Language", "NotificationTypeId", "TemplateMessage", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[,]
                {
                    { new Guid("26c8d3fb-352d-4b5e-9022-0ddef3f18b7c"), new DateTime(2025, 7, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "fr-FR", new Guid("edc32213-8546-4e0b-ab27-b909fb03c87e"), "📢 Un nouveau sondage est disponible ! Veuillez le remplir dès maintenant afin de nous aider à mieux vous comprendre et recevez des points FSEL en récompense ! 🎯", null, null, null },
                    { new Guid("2ee0cbb4-f4dc-4403-a906-3360fb1c85ae"), new DateTime(2025, 7, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "en-US", new Guid("edc32213-8546-4e0b-ab27-b909fb03c87e"), "📢 You have a new survey available! Complete the survey now to help us better understand you and receive FSEL points as a reward! 🎯", null, null, null },
                    { new Guid("ab66276d-1784-41c9-a5fb-60334a2e101f"), new DateTime(2025, 7, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "vi-VN", new Guid("edc32213-8546-4e0b-ab27-b909fb03c87e"), "📢 Bạn có một bài khảo sát mới! Hoàn thành bài khảo sát ngay để giúp chúng tôi hiểu hơn về bạn và nhận xu FSEL! 🎯", null, null, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("26c8d3fb-352d-4b5e-9022-0ddef3f18b7c"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("2ee0cbb4-f4dc-4403-a906-3360fb1c85ae"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("ab66276d-1784-41c9-a5fb-60334a2e101f"));

            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("edc32213-8546-4e0b-ab27-b909fb03c87e"));
        }
    }
}
