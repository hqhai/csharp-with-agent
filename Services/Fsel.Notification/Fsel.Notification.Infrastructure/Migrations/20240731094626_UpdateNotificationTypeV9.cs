using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Notification.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateNotificationTypeV9 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("59942527-ca1f-4f18-a66b-cdcfe87b317e"),
                column: "TemplateMessage",
                value: "Bài đăng của bạn đã được chấm bởi hệ thống AI của FSEL. Nhấn để xem chi tiết");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("59942527-ca1f-4f18-a66b-cdcfe87b317e"),
                column: "TemplateMessage",
                value: "Bài viết của bạn đã được hệ thống AI ChatGPT nhận xét. Nhấn để xem chi tiết");
        }
    }
}
