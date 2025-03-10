using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Fsel.Notification.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRecallCoinSurvey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "NotificationTypes",
                columns: new[] { "Id", "Content", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "Icon", "IsDeleted", "Priority", "TemplateLink", "TemplateMessage", "Type", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[] { new Guid("46540fc5-a4fd-4dac-bb8f-ef86698528dc"), "RecallCoinSurvey", new DateTime(2025, 2, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, 1, "", "Chúng tôi đã phát hiện một số tài khoản lợi dụng lỗi hệ thống khi làm khảo sát đầu vào (survey) để giành một số coins trái với quy định. Số coins và vật phẩm quy đổi từ đó sẽ bị thu hồi. Hãy tuân thủ quy định để tránh bị cấm tài khoản!", "Text", null, null, null });

            migrationBuilder.InsertData(
                table: "NotificationTypeTranslations",
                columns: new[] { "Id", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "IsDeleted", "Language", "NotificationTypeId", "TemplateMessage", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[,]
                {
                    { new Guid("1867ebca-aa7d-4fbc-9de9-8e432bb3b87a"), new DateTime(2025, 2, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "en-US", new Guid("46540fc5-a4fd-4dac-bb8f-ef86698528dc"), "We have detected some accounts exploiting a system error in the entry survey to gain coins in violation of the regulations. The coins and any redeemed items obtained through this method will be revoked. Please follow the regulations to avoid account suspension!", null, null, null },
                    { new Guid("9ac2883d-b2ad-4d94-a724-91494c1e5b5f"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "vi-VN", new Guid("46540fc5-a4fd-4dac-bb8f-ef86698528dc"), "Chúng tôi đã phát hiện một số tài khoản lợi dụng lỗi hệ thống khi làm khảo sát đầu vào (survey) để giành một số coins trái với quy định. Số coins và vật phẩm quy đổi từ đó sẽ bị thu hồi. Hãy tuân thủ quy định để tránh bị cấm tài khoản!", null, null, null },
                    { new Guid("f6943cc3-15c1-4849-a852-309be6d17eb4"), new DateTime(2025, 2, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "fr-FR", new Guid("46540fc5-a4fd-4dac-bb8f-ef86698528dc"), "Nous avons détecté certains comptes exploitant une erreur du système lors du sondage d'entrée pour obtenir des pièces en violation des règles. Les pièces et les objets échangés de cette manière seront annulés. Veuillez respecter les règlements afin d’éviter la suspension de votre compte !", null, null, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("1867ebca-aa7d-4fbc-9de9-8e432bb3b87a"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("9ac2883d-b2ad-4d94-a724-91494c1e5b5f"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("f6943cc3-15c1-4849-a852-309be6d17eb4"));

            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("46540fc5-a4fd-4dac-bb8f-ef86698528dc"));
        }
    }
}
