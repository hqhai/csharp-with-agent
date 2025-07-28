using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Notification.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateNotificationTypeV17 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("4a1f848c-f666-48fa-b127-c871af142332"),
                column: "TemplateLink",
                value: "/account/mySubscription");

            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("a99bfaae-4139-4766-9405-ef24332fb51a"),
                column: "TemplateLink",
                value: "/account/mySubscription");

            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("caca0a25-2196-4536-a778-8e08d605691f"),
                column: "TemplateLink",
                value: "/account/mySubscription");

            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("f9d5a75f-fd55-4b1e-ade8-a446b6465e34"),
                column: "TemplateLink",
                value: "/account/mySubscription");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("4a1f848c-f666-48fa-b127-c871af142332"),
                column: "TemplateLink",
                value: "/change-level");

            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("a99bfaae-4139-4766-9405-ef24332fb51a"),
                column: "TemplateLink",
                value: "/account?setting=mySubscription");

            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("caca0a25-2196-4536-a778-8e08d605691f"),
                column: "TemplateLink",
                value: "/account?setting=mySubscription");

            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("f9d5a75f-fd55-4b1e-ade8-a446b6465e34"),
                column: "TemplateLink",
                value: "/account?setting=mySubscription");
        }
    }
}
