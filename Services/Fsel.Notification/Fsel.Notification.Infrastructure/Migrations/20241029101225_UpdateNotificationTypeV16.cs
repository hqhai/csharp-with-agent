using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Notification.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateNotificationTypeV16 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("aa0bc23b-2281-4e19-bdff-10c87dfb6152"),
                column: "TemplateMessage",
                value: "Welcome to FSEL! Get ready for your learning journey over the next {0} months!");

            migrationBuilder.UpdateData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("ebbd7083-4e2e-4f1f-bb6a-faebb371a3ce"),
                column: "TemplateMessage",
                value: "Bienvenue chez FSEL ! Préparez-vous pour votre parcours d'apprentissage au cours des {0} prochains mois !");

            migrationBuilder.UpdateData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("fcf43e61-7a49-4978-b547-85de6aea5363"),
                column: "TemplateMessage",
                value: "Chào mừng bạn đến với FSEL! Hãy chuẩn bị sẵn sàng cho hành trình học tập trong vòng {0} tháng tới nhé!");

            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("30f3dce8-e8c0-4231-a2b1-8b97e9551129"),
                column: "TemplateMessage",
                value: "Chào mừng bạn đến với FSEL! Hãy chuẩn bị sẵn sàng cho hành trình học tập trong vòng {0} tháng tới nhé!");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("aa0bc23b-2281-4e19-bdff-10c87dfb6152"),
                column: "TemplateMessage",
                value: "Welcome to FSEL! Get ready for your learning journey over the next 3 months!");

            migrationBuilder.UpdateData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("ebbd7083-4e2e-4f1f-bb6a-faebb371a3ce"),
                column: "TemplateMessage",
                value: "Bienvenue chez FSEL ! Préparez-vous pour votre parcours d'apprentissage au cours des 3 prochains mois !");

            migrationBuilder.UpdateData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("fcf43e61-7a49-4978-b547-85de6aea5363"),
                column: "TemplateMessage",
                value: "Chào mừng bạn đến với FSEL! Hãy chuẩn bị sẵn sàng cho hành trình học tập trong vòng 24 tháng tới nhé!");

            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("30f3dce8-e8c0-4231-a2b1-8b97e9551129"),
                column: "TemplateMessage",
                value: "Chào mừng bạn đến với FSEL! Hãy chuẩn bị sẵn sàng cho hành trình học tập trong vòng 24 tháng tới nhé!");
        }
    }
}
