using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

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
                value: "/learn/{0}?courseId={1}&unitId={0}&type=classForum&classForumResultId={2}&commentId={3}");

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
                value: "/learn/{0}?courseId={1}&unitId={0}&type=classForum&classForumResultId={1}&commentId={2}&repCommentId={3}");

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
                value: "/learn/{0}?courseId={1}&unitId={0}&type=classForum&classForumResultId={2}");

            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("d789788a-1ba6-405b-f280-ae92cd3b4fc2"),
                column: "TemplateLink",
                value: "/class-forum-management");

            migrationBuilder.InsertData(
                table: "NotificationTypes",
                columns: new[] { "Id", "Content", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "Icon", "IsDeleted", "Priority", "TemplateLink", "TemplateMessage", "Type", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[,]
                {
                    { new Guid("17c0df26-f131-42bf-b315-0a8a85524f6f"), "DeleteClassForumResult", new DateTime(2023, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, 1, "/learn/{0}?courseId={1}&unitId={0}&type=classForum&classForumResultId={2}", "Bài viết của bạn đã vi phạm Tiêu chuẩn cộng đồng của FSEL và đã bị xóa", "LinkPage", null, null, null },
                    { new Guid("96c0df96-f131-42bf-b315-0a8a85524f6f"), "DeleteComment", new DateTime(2023, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, 1, "/learn/{0}?courseId={1}&unitId={0}&type=classForum&classForumResultId={2}&commentId={3}", "Bình luận của bạn đã vi phạm Tiêu chuẩn cộng đồng của FSEL và đã bị xóa", "LinkComment", null, null, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("17c0df26-f131-42bf-b315-0a8a85524f6f"));

            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("96c0df96-f131-42bf-b315-0a8a85524f6f"));

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
        }
    }
}
