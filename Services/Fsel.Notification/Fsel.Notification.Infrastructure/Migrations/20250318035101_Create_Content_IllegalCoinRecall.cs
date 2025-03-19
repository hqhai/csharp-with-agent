using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Fsel.Notification.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Create_Content_IllegalCoinRecall : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "NotificationTypes",
                columns: new[] { "Id", "Content", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "Icon", "IsDeleted", "Priority", "TemplateLink", "TemplateMessage", "Type", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[] { new Guid("50947e80-1900-4fa5-89d2-f8bd309660da"), "IllegalCoinRecall", new DateTime(2025, 2, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, 1, "", "Chúng tôi đã phát hiện một số tài khoản lợi dụng chức năng giới thiệu bạn bè để giành một số coins trái với quy định. Số coins và vật phẩm quy đổi từ đó sẽ bị thu hồi. Hãy tuân thủ quy định để tránh bị cấm tài khoản!", "Text", null, null, null });

            migrationBuilder.InsertData(
                table: "NotificationTypeTranslations",
                columns: new[] { "Id", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "IsDeleted", "Language", "NotificationTypeId", "TemplateMessage", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[,]
                {
                    { new Guid("61d84d68-3be8-46df-a771-a3037cbd86ee"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "vi-VN", new Guid("50947e80-1900-4fa5-89d2-f8bd309660da"), "Chúng tôi đã phát hiện một số tài khoản lợi dụng chức năng giới thiệu bạn bè để giành một số coins trái với quy định. Số coins và vật phẩm quy đổi từ đó sẽ bị thu hồi. Hãy tuân thủ quy định để tránh bị cấm tài khoản!", null, null, null },
                    { new Guid("6584662f-04a7-43e7-86d0-d605fd827442"), new DateTime(2025, 2, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "en-US", new Guid("50947e80-1900-4fa5-89d2-f8bd309660da"), "We have detected that some accounts have exploited the referral function to obtain coins in violation of the rules. The coins and items redeemed from them will be revoked. Please follow the rules to avoid account suspension!", null, null, null },
                    { new Guid("72cba562-2bee-4d33-903c-da9e27e64d48"), new DateTime(2025, 2, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "fr-FR", new Guid("50947e80-1900-4fa5-89d2-f8bd309660da"), "Nous avons détecté que certains comptes ont abusé de la fonction de parrainage pour obtenir des pièces de manière non conforme aux règles. Les pièces et les objets échangés seront récupérés. Veuillez respecter les règles pour éviter l'interdiction de votre compte !", null, null, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("61d84d68-3be8-46df-a771-a3037cbd86ee"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("6584662f-04a7-43e7-86d0-d605fd827442"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("72cba562-2bee-4d33-903c-da9e27e64d48"));

            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("50947e80-1900-4fa5-89d2-f8bd309660da"));
        }
    }
}
