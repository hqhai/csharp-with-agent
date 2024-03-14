using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Fsel.Notification.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateNotificationTypeV5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("810c70ca-4f3c-20d4-a491-708fa45d42ea"));

            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("810c70ca-4f3c-4d02-a491-708fa45d42ea"),
                column: "TemplateMessage",
                value: "{0} đã thích bài viết của bạn.");

            migrationBuilder.InsertData(
                table: "NotificationTypes",
                columns: new[] { "Id", "Content", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "Icon", "IsDeleted", "Priority", "TemplateLink", "TemplateMessage", "Type", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[,]
                {
                    { new Guid("1a7f9999-79e2-4eb1-85a8-cc8d1398f732"), "ReviewFsel", new DateTime(2023, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, 1, "", "Hãy chia sẻ cảm nhận của bạn về LMS!", "LinkPage", null, null, null },
                    { new Guid("3ad02de7-a8a8-4db6-8286-486c81390ed3"), "LikeComment", new DateTime(2023, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, 1, "/learn/{0}?courseId={1}&unitId={0}&type=classForum&classForumResultId={2}&commentId={3}", "{0} đã thích bình luận của bạn.", "LinkComment", null, null, null },
                    { new Guid("4b5173d1-3d0b-4df6-b3ca-15cd175926a1"), "ChangeClassLiveTeacher", new DateTime(2023, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, 1, "", "Buổi {0} diễn ra vào ngày {1} đã thay đổi giáo viên", "LinkPopup", null, null, null },
                    { new Guid("59bc5071-1ef7-4362-9bb0-8848bb6f8ced"), "DiscussionBoardInActive", new DateTime(2023, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, 1, "", "Bay! Bay cao! Bay xa! Hãy cùng khám phá với các bạn trên khắp thế giới qua Diễn đàn chung nào!", "LinkPage", null, null, null },
                    { new Guid("85062a12-ab5b-486c-a8ab-db329ea7f46d"), "MockTest", new DateTime(2023, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, 1, "", "Bài {0} đã được giáo viên chấm điểm. Nhấn để xem chi tiết", "LinkPage", null, null, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("1a7f9999-79e2-4eb1-85a8-cc8d1398f732"));

            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("3ad02de7-a8a8-4db6-8286-486c81390ed3"));

            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("4b5173d1-3d0b-4df6-b3ca-15cd175926a1"));

            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("59bc5071-1ef7-4362-9bb0-8848bb6f8ced"));

            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("85062a12-ab5b-486c-a8ab-db329ea7f46d"));

            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("810c70ca-4f3c-4d02-a491-708fa45d42ea"),
                column: "TemplateMessage",
                value: "{0} vừa thích bài viết của bạn.");

            migrationBuilder.InsertData(
                table: "NotificationTypes",
                columns: new[] { "Id", "Content", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "Icon", "IsDeleted", "Priority", "TemplateLink", "TemplateMessage", "Type", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[] { new Guid("810c70ca-4f3c-20d4-a491-708fa45d42ea"), "LikeComment", new DateTime(2023, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, 1, "/learn/{0}?courseId={1}&unitId={0}&type=classForum&classForumResultId={2}&commentId={3}", "{0} vừa thích bình luận của bạn.", "LinkComment", null, null, null });
        }
    }
}
