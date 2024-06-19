using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Fsel.Notification.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_NotificationTypeTable_Add_TemplateLink_V2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("efba1f99-8fad-47b5-af14-978f674f438e"),
                column: "TemplateLink",
                value: "/learn/{0}?courseId={1}&unitId={0}&type=classForum&classForumResultId={2}&commentId={3}");

            migrationBuilder.InsertData(
                table: "NotificationTypes",
                columns: new[] { "Id", "Content", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "Icon", "IsDeleted", "Priority", "TemplateLink", "TemplateMessage", "Type", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[,]
                {
                    { new Guid("810c70ca-4f3c-20d4-a491-708fa45d42ea"), "LikeComment", new DateTime(2023, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, 1, "/learn/{0}?courseId={1}&unitId={0}&type=classForum&classForumResultId={2}&commentId={3}", "{0} vừa thích bình luận của bạn.", "LinkComment", null, null, null },
                    { new Guid("810c70ca-4f3c-4d02-a491-708fa45d42ea"), "LikeClassForum", new DateTime(2023, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, 1, "/learn/{0}?courseId={1}&unitId={0}&type=classForum&classForumResultId={2}", "{0} vừa thích bài viết của bạn.", "LinkPage", null, null, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("810c70ca-4f3c-20d4-a491-708fa45d42ea"));

            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("810c70ca-4f3c-4d02-a491-708fa45d42ea"));

            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("efba1f99-8fad-47b5-af14-978f674f438e"),
                column: "TemplateLink",
                value: "/learn/{0}?courseId={1}&unitId={0}&type=classForum&classForumResultId={1}&commentId={2}");
        }
    }
}
