using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Notification.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_UserTable_AddIndexNotificationMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("4e496aad-c2ce-42a2-8971-71cd28cf3fae"),
                column: "TemplateMessage",
                value: "{0} đã đăng kí thành công gói học FSEL,bạn vừa nhận được {1} xu, nhấn để xem chi tiết ");

            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("5e7e39f8-750a-4709-82b2-12805fc9e2e9"),
                column: "TemplateLink",
                value: "");

            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("a3d6129b-9844-448d-aa1c-d6155b89ce6b"),
                column: "TemplateLink",
                value: "");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationMessages_IsDeleted_UserId_SenderId",
                table: "NotificationMessages",
                columns: new[] { "IsDeleted", "UserId", "SenderId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_NotificationMessages_IsDeleted_UserId_SenderId",
                table: "NotificationMessages");

            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("4e496aad-c2ce-42a2-8971-71cd28cf3fae"),
                column: "TemplateMessage",
                value: "{0} đã đăng kí thành công gói học FSEL,bạn vừa nhận được {1}🟡, nhấn để xem chi tiết ");

            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("5e7e39f8-750a-4709-82b2-12805fc9e2e9"),
                column: "TemplateLink",
                value: "/invite-friends");

            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("a3d6129b-9844-448d-aa1c-d6155b89ce6b"),
                column: "TemplateLink",
                value: "/invite-friends");
        }
    }
}
