using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Notification.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_Config_NotificationType_UpgradeOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("81fbb72a-ab26-44b4-824a-ff28b7edc276"),
                column: "TemplateMessage",
                value: "Việc học liên tục có thể giúp cải thiện các kỹ năng của bạn nhanh gấp 3 lần so với các bạn học dừng lại. Nhấn để nâng cấp ngay nào!");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("81fbb72a-ab26-44b4-824a-ff28b7edc276"),
                column: "TemplateMessage",
                value: "Chúc mừng bạn đã hoàn thành khoá học {0}. Nhấn để xem lại hành trình của bạn theo góc nhìn tổng quan nhé.");
        }
    }
}
