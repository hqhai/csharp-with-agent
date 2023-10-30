using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Notification.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_NotificationTypeTable_Add_TemplateLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("810c70ca-4f3c-4d02-a194-708fa45d42ea"),
                column: "TemplateLink",
                value: "/learn/{0}?courseId={1}&unitId={0}&type=classForum&classForumResultId={3}&commentId={4}");

            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("86c0df96-f131-42bf-b315-0a8a85584f6a"),
                column: "TemplateLink",
                value: "/payment");

            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("9e983172-dba2-4ca7-b602-372c011ecb99"),
                column: "TemplateLink",
                value: "/learn/{0}?courseId={1}&unitId={2}&type=classForum&classForumResultId={3}&repCommentId={4}");

            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("a805f268-467b-4149-8d01-b2ea05e1f3bd"),
                column: "TemplateLink",
                value: "/learn");

            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("d789788a-1ba5-405b-b280-ae92cd3b4fc2"),
                column: "TemplateLink",
                value: "/learn/{0}?courseId={1}&unitId={0}&type=classForum&classForumResultId={3}");

            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("d789788a-1ba6-405b-f280-ae92cd3b4fc2"),
                column: "TemplateLink",
                value: "/class-forum-management");

            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("efba1f99-8fad-47b5-af14-978f674f438e"),
                column: "TemplateLink",
                value: "/learn/{0}?courseId={1}&unitId={0}&type=classForum&classForumResultId={2}&commentId={3}");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("810c70ca-4f3c-4d02-a194-708fa45d42ea"),
                column: "TemplateLink",
                value: "");

            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("86c0df96-f131-42bf-b315-0a8a85584f6a"),
                column: "TemplateLink",
                value: "");

            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("9e983172-dba2-4ca7-b602-372c011ecb99"),
                column: "TemplateLink",
                value: "");

            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("a805f268-467b-4149-8d01-b2ea05e1f3bd"),
                column: "TemplateLink",
                value: "");

            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("d789788a-1ba5-405b-b280-ae92cd3b4fc2"),
                column: "TemplateLink",
                value: "");

            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("d789788a-1ba6-405b-f280-ae92cd3b4fc2"),
                column: "TemplateLink",
                value: "");

            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("efba1f99-8fad-47b5-af14-978f674f438e"),
                column: "TemplateLink",
                value: "");
        }
    }
}
