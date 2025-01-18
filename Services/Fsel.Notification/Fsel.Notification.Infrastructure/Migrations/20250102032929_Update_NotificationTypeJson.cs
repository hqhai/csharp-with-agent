using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Notification.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_NotificationTypeJson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("06beee4e-1723-4079-8cf4-ede3f755b2f7"),
                column: "TemplateMessage",
                value: "{0} a terminé l'Unité 1, et vous venez de recevoir {1} xu. Appuyez pour voir les détails.");

            migrationBuilder.UpdateData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("22e0b002-3122-4d83-b238-7496153cba03"),
                column: "TemplateMessage",
                value: "{0} đã hoàn thành unit 1,bạn vừa nhận được {1} xu, nhấn để xem chi tiết ");

            migrationBuilder.UpdateData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("41150d47-6270-4420-8a08-627bf4babd45"),
                column: "TemplateMessage",
                value: "{0} has completed the placement test, and you have just received {1} xu. Tap to see details.");

            migrationBuilder.UpdateData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("4acdfde0-273f-40cc-8f21-b05ab07921b4"),
                column: "TemplateMessage",
                value: "{0} has completed Unit 1, and you have just received {1} xu. Tap to see details");

            migrationBuilder.UpdateData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("593ca78c-074b-4da6-bc31-421a672c04ec"),
                column: "TemplateMessage",
                value: "{0} a terminé le test de positionnement, et vous venez de recevoir {1} xu. Appuyez pour voir les détails.");

            migrationBuilder.UpdateData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("ab10dee3-6d3b-4482-92ea-65dcac210fe0"),
                column: "TemplateMessage",
                value: "{0} đã hoàn thành bài kiểm tra,bạn vừa nhận được {1} xu, nhấn để xem chi tiết");

            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("5e7e39f8-750a-4709-82b2-12805fc9e2e9"),
                column: "TemplateMessage",
                value: "{0} đã hoàn thành bài kiểm tra,bạn vừa nhận được {1} xu, nhấn để xem chi tiết");

            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("a3d6129b-9844-448d-aa1c-d6155b89ce6b"),
                column: "TemplateMessage",
                value: "{0} đã hoàn thành unit 1,bạn vừa nhận được {1} xu, nhấn để xem chi tiết ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("06beee4e-1723-4079-8cf4-ede3f755b2f7"),
                column: "TemplateMessage",
                value: "{0} a terminé l'Unité 1, et vous venez de recevoir 500 xu. Appuyez pour voir les détails.");

            migrationBuilder.UpdateData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("22e0b002-3122-4d83-b238-7496153cba03"),
                column: "TemplateMessage",
                value: "{0} đã hoàn thành unit 1,bạn vừa nhận được 500 xu, nhấn để xem chi tiết ");

            migrationBuilder.UpdateData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("41150d47-6270-4420-8a08-627bf4babd45"),
                column: "TemplateMessage",
                value: "{0} has completed the placement test, and you have just received 100 xu. Tap to see details.");

            migrationBuilder.UpdateData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("4acdfde0-273f-40cc-8f21-b05ab07921b4"),
                column: "TemplateMessage",
                value: "{0} has completed Unit 1, and you have just received 500 xu. Tap to see details");

            migrationBuilder.UpdateData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("593ca78c-074b-4da6-bc31-421a672c04ec"),
                column: "TemplateMessage",
                value: "{0} a terminé le test de positionnement, et vous venez de recevoir 100 xu. Appuyez pour voir les détails.");

            migrationBuilder.UpdateData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("ab10dee3-6d3b-4482-92ea-65dcac210fe0"),
                column: "TemplateMessage",
                value: "{0} đã hoàn thành bài kiểm tra,bạn vừa nhận được 100 xu, nhấn để xem chi tiết");

            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("5e7e39f8-750a-4709-82b2-12805fc9e2e9"),
                column: "TemplateMessage",
                value: "{0} đã hoàn thành bài kiểm tra,bạn vừa nhận được 100 xu, nhấn để xem chi tiết");

            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("a3d6129b-9844-448d-aa1c-d6155b89ce6b"),
                column: "TemplateMessage",
                value: "{0} đã hoàn thành unit 1,bạn vừa nhận được 500 xu, nhấn để xem chi tiết ");
        }
    }
}
