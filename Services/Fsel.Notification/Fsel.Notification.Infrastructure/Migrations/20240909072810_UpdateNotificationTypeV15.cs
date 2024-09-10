using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Notification.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateNotificationTypeV15 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("57c709a6-03bd-4a3f-ad34-f9449587e1df"),
                column: "TemplateLink",
                value: "learn/{0}/{1}/lesson/{2}?page=class-forum");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("57c709a6-03bd-4a3f-ad34-f9449587e1df"),
                column: "TemplateLink",
                value: "learn/{0}/{1}/lesson/{3}?page=class-forum");
        }
    }
}
