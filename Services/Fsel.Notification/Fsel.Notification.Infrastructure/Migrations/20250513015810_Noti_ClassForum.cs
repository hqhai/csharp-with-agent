using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Fsel.Notification.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Noti_ClassForum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "NotificationTypes",
                columns: new[] { "Id", "Content", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "Icon", "IsDeleted", "Priority", "TemplateLink", "TemplateMessage", "Type", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[,]
                {
                    { new Guid("02a9a4a2-c266-494c-8144-6da4f1270fb8"), "ForbiddenClassForum", new DateTime(2025, 2, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, 1, "", "Bài làm của bạn trong {0} đã bị từ chối phê duyệt do vi phạm tiêu chuẩn cộng đồng của FSEL!", "LinkPage", null, null, null },
                    { new Guid("41bcd8b6-a17b-4c15-ba75-f3b86301a780"), "LanguageNotEnglish", new DateTime(2025, 2, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, 1, "", "Oops! Có vẻ như bài làm của bạn chưa đúng ngôn ngữ yêu cầu, do đó không thể đăng lên diễn đàn!", "LinkPage", null, null, null },
                    { new Guid("898eb157-9c97-4298-bc26-d913ace71ac6"), "NullForbiddenClassForum", new DateTime(2025, 2, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, 1, "", "Hmm… có vẻ nội dung này hơi đặc biệt 😄! Tạm thời chúng tôi chưa thể hiển thị kết quả kiểm duyệt. Cảm ơn bạn đã kiên nhẫn.", "LinkPage", null, null, null }
                });

            migrationBuilder.InsertData(
                table: "NotificationTypeTranslations",
                columns: new[] { "Id", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "IsDeleted", "Language", "NotificationTypeId", "TemplateMessage", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[,]
                {
                    { new Guid("095e36a3-dc40-4687-81b3-2581d72e4cdc"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "vi-VN", new Guid("41bcd8b6-a17b-4c15-ba75-f3b86301a780"), "Oops! Có vẻ như bài làm của bạn chưa đúng ngôn ngữ yêu cầu, do đó không thể đăng lên diễn đàn!", null, null, null },
                    { new Guid("375b09b2-872c-4307-b880-3c0451ed96d8"), new DateTime(2025, 2, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "fr-FR", new Guid("02a9a4a2-c266-494c-8144-6da4f1270fb8"), "Votre publication dans {0} a été refusée en raison de la violation des normes de la communauté FSEL!", null, null, null },
                    { new Guid("88eac705-f68c-49f4-b55b-759dbe8000a3"), new DateTime(2025, 2, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "fr-FR", new Guid("41bcd8b6-a17b-4c15-ba75-f3b86301a780"), "Oups ! Il semble que votre publication ne soit pas dans la langue requise, elle ne peut donc pas être publiée sur le forum !", null, null, null },
                    { new Guid("a9639c2e-2c0a-49ac-8fa4-99024385c887"), new DateTime(2025, 2, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "fr-FR", new Guid("898eb157-9c97-4298-bc26-d913ace71ac6"), "Hmm… il semble que ce contenu soit un peu particulier 😄 ! Nous ne pouvons pas afficher le résultat de la modération pour le moment. Merci pour votre patience.", null, null, null },
                    { new Guid("b1679656-3de9-4d02-af1d-98fb32af3163"), new DateTime(2025, 2, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "en-US", new Guid("41bcd8b6-a17b-4c15-ba75-f3b86301a780"), "Oops! It looks like your post is not in the required language, so it cannot be published on the Class forum!", null, null, null },
                    { new Guid("b390f7ca-2927-4b01-bb6d-3fa4d27586a2"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "vi-VN", new Guid("02a9a4a2-c266-494c-8144-6da4f1270fb8"), "Bài làm của bạn trong {0} đã bị từ chối phê duyệt do vi phạm tiêu chuẩn cộng đồng của FSEL!", null, null, null },
                    { new Guid("b5a18145-74d7-4b53-b12c-f381f42f0d19"), new DateTime(2025, 2, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "en-US", new Guid("02a9a4a2-c266-494c-8144-6da4f1270fb8"), "Your post in {0} has been denied approval due to violating FSEL's community standards!", null, null, null },
                    { new Guid("ee2def86-f2df-4461-ba84-54e3619432c1"), new DateTime(2025, 2, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "en-US", new Guid("898eb157-9c97-4298-bc26-d913ace71ac6"), "Hmm… it seems this content is a bit special 😄! We're temporarily unable to display the moderation result. Thank you for your patience.", null, null, null },
                    { new Guid("eec1fdce-c276-48ce-98b7-0e43ffab666d"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "vi-VN", new Guid("898eb157-9c97-4298-bc26-d913ace71ac6"), "Hmm… có vẻ nội dung này hơi đặc biệt 😄! Tạm thời chúng tôi chưa thể hiển thị kết quả kiểm duyệt. Cảm ơn bạn đã kiên nhẫn.", null, null, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("095e36a3-dc40-4687-81b3-2581d72e4cdc"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("375b09b2-872c-4307-b880-3c0451ed96d8"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("88eac705-f68c-49f4-b55b-759dbe8000a3"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("a9639c2e-2c0a-49ac-8fa4-99024385c887"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("b1679656-3de9-4d02-af1d-98fb32af3163"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("b390f7ca-2927-4b01-bb6d-3fa4d27586a2"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("b5a18145-74d7-4b53-b12c-f381f42f0d19"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("ee2def86-f2df-4461-ba84-54e3619432c1"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("eec1fdce-c276-48ce-98b7-0e43ffab666d"));

            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("02a9a4a2-c266-494c-8144-6da4f1270fb8"));

            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("41bcd8b6-a17b-4c15-ba75-f3b86301a780"));

            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("898eb157-9c97-4298-bc26-d913ace71ac6"));
        }
    }
}
