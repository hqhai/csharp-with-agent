using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Fsel.Notification.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Create_Content_SurveyReward : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "NotificationTypes",
                columns: new[] { "Id", "Content", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "Icon", "IsDeleted", "Priority", "TemplateLink", "TemplateMessage", "Type", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[] { new Guid("3c7f9946-3f95-4a84-b2df-7cffbf36a41c"), "SurveyReward", new DateTime(2025, 2, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, 1, "", "Cảm ơn bạn đã tham gia khảo sát! Chúng tôi trân trọng cảm ơn bạn đã hoàn thành khảo sát Tháng Tự Học Ngoại Ngữ - Hà Nội! Để bày tỏ sự tri ân, bạn đã nhận được {0} FSEL COINS. Chúng tôi sẽ xem xét và đánh giá ý kiến của bạn để không ngừng cải thiện trải nghiệm học tập. Cảm ơn bạn đã đồng hành cùng FSEL!", "Text", null, null, null });

            migrationBuilder.InsertData(
                table: "NotificationTypeTranslations",
                columns: new[] { "Id", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "IsDeleted", "Language", "NotificationTypeId", "TemplateMessage", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[,]
                {
                    { new Guid("13dd85c7-014c-4a32-9392-92a46440c481"), new DateTime(2025, 2, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "en-US", new Guid("3c7f9946-3f95-4a84-b2df-7cffbf36a41c"), "Thank you for participating in the survey! We sincerely appreciate your completion of the Self-Study Language Month - Hanoi survey! As a token of our gratitude, you have received {0} FSEL COINS. Your feedback will be carefully reviewed and evaluated to continuously improve the learning experience. Thank you for your support and for being part of FSEL!  ", null, null, null },
                    { new Guid("788c4de6-030b-4866-8e91-3fb0e913f670"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "vi-VN", new Guid("3c7f9946-3f95-4a84-b2df-7cffbf36a41c"), "Cảm ơn bạn đã tham gia khảo sát! Chúng tôi trân trọng cảm ơn bạn đã hoàn thành khảo sát Tháng Tự Học Ngoại Ngữ - Hà Nội! Để bày tỏ sự tri ân, bạn đã nhận được {0} FSEL COINS. Chúng tôi sẽ xem xét và đánh giá ý kiến của bạn để không ngừng cải thiện trải nghiệm học tập. Cảm ơn bạn đã đồng hành cùng FSEL!", null, null, null },
                    { new Guid("8592527a-e771-4e37-a2e0-e5e5aa5ff667"), new DateTime(2025, 2, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "fr-FR", new Guid("3c7f9946-3f95-4a84-b2df-7cffbf36a41c"), "Merci d'avoir participé à l'enquête ! Nous vous remercions sincèrement d'avoir complété l'enquête sur le Mois de l'Auto-apprentissage des Langues - Hanoï ! En signe de notre gratitude, vous avez reçu {0} FSEL COINS. Vos retours seront attentivement examinés et évalués afin d'améliorer continuellement l'expérience d'apprentissage. Merci pour votre soutien et votre engagement avec FSEL ! ", null, null, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("13dd85c7-014c-4a32-9392-92a46440c481"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("788c4de6-030b-4866-8e91-3fb0e913f670"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("8592527a-e771-4e37-a2e0-e5e5aa5ff667"));

            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("3c7f9946-3f95-4a84-b2df-7cffbf36a41c"));
        }
    }
}
