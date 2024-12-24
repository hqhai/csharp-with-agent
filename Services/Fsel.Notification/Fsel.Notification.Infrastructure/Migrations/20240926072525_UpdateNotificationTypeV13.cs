using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Notification.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateNotificationTypeV13 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("59942527-ca1f-4f18-a66b-cdcfe87b317e"),
                column: "TemplateLink",
                value: "learn/{0}/{1}/lesson/{2}?page=class-forum");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("59942527-ca1f-4f18-a66b-cdcfe87b317e"),
                column: "TemplateLink",
                value: "/learn/lesson/{0}?courseId={1}&unitId={2}?type=aiFeedback");
        }
    }
}
