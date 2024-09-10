using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Notification.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateNotificationTypeV14 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("27007edb-25d8-493e-84d8-85f6f9e165b2"),
                column: "TemplateLink",
                value: "learn/{0}/{1}/lesson/{2}/class-forum/{3}?page=class-forum");

            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("57c709a6-03bd-4a3f-ad34-f9449587e1df"),
                column: "TemplateLink",
                value: "learn/{0}/{1}/lesson/{3}?page=class-forum");

            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("69cc9e39-82d5-4d1a-81b9-8c97ec54bfad"),
                column: "TemplateLink",
                value: "learn/{0}/{1}/lesson/{2}/class-forum/{3}?page=class-forum");

            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("75540141-a6d9-409d-a3ce-90c9a7577e8a"),
                column: "TemplateLink",
                value: "learn/{0}/{1}/lesson/{2}?page=class-forum");

            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("810c70ca-4f3c-4d02-a194-708fa45d42ea"),
                column: "TemplateLink",
                value: "learn/{0}/{1}/lesson/{2}/class-forum/{3}?page=class-forum");

            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("810c70ca-4f3c-4d02-a491-708fa45d42ea"),
                column: "TemplateLink",
                value: "learn/{0}/{1}/lesson/{2}/class-forum/{3}?page=class-forum");

            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("9e983172-dba2-4ca7-b602-372c011ecb99"),
                column: "TemplateLink",
                value: "learn/{0}/{1}/lesson/{2}/class-forum/{3}?page=class-forum-user");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("27007edb-25d8-493e-84d8-85f6f9e165b2"),
                column: "TemplateLink",
                value: "/learn/lesson/{0}?courseId={1}&unitId={2}&type=class-forum-comment&resultId={3}");

            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("57c709a6-03bd-4a3f-ad34-f9449587e1df"),
                column: "TemplateLink",
                value: "/learn/lesson/{0}?courseId={1}&unitId={2}&type=class-forum");

            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("69cc9e39-82d5-4d1a-81b9-8c97ec54bfad"),
                column: "TemplateLink",
                value: "/learn/lesson/{0}?courseId={1}&unitId={2}&type=class-forum");

            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("75540141-a6d9-409d-a3ce-90c9a7577e8a"),
                column: "TemplateLink",
                value: "/learn/lesson/{0}?courseId={1}&unitId={2}&type=class-forum");

            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("810c70ca-4f3c-4d02-a194-708fa45d42ea"),
                column: "TemplateLink",
                value: "/learn/lesson/{0}?courseId={1}&unitId={2}&type=class-forum-comment&resultId={3}");

            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("810c70ca-4f3c-4d02-a491-708fa45d42ea"),
                column: "TemplateLink",
                value: "/learn/lesson/{0}?courseId={1}&unitId={2}&type=class-forum");

            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("9e983172-dba2-4ca7-b602-372c011ecb99"),
                column: "TemplateLink",
                value: "/learn/lesson/{0}?courseId={1}&unitId={2}&type=class-forum-comment&resultId={3}");
        }
    }
}
