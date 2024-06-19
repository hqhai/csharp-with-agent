using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Notification.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDataTableNotificationTypeV4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.InsertData(
                table: "NotificationTypes",
                columns: new[] { "Id", "Content", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "Icon", "IsDeleted", "Priority", "TemplateLink", "TemplateMessage", "Type", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[] { new Guid("59942527-ca1f-4f18-a66b-cdcfe87b317e"), "AIFeedBack", new DateTime(2023, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, 1, "", "Bài viết của bạn đã được hệ thống AI ChatGPT nhận xét. Nhấn để xem chi tiết", "LinkPage", null, null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("59942527-ca1f-4f18-a66b-cdcfe87b317e"));
        }
    }
}
