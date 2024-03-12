using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Fsel.Notification.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateNotificationTypeV6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("17c0df26-f131-42bf-b315-0a8a85524f6f"));

            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("86c0df96-f131-42bf-b315-0a8a85584f6a"));

            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("96c0df96-f131-42bf-b315-0a8a85524f6f"));

            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("a805f268-467b-4149-8d01-b2ea05e1f3bd"));

            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("d789788a-1ba6-405b-f280-ae92cd3b4fc2"));

            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("1a7f9999-79e2-4eb1-85a8-cc8d1398f732"),
                column: "TemplateLink",
                value: "/review");

            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("59942527-ca1f-4f18-a66b-cdcfe87b317e"),
                column: "TemplateLink",
                value: "/learn/lesson/{0}?courseId={1}&unitId={2}?type=aiFeedback");

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
                keyValue: new Guid("85062a12-ab5b-486c-a8ab-db329ea7f46d"),
                column: "TemplateLink",
                value: "/learn/mock-test-report?sectionGroupId={0}&mockTestResultId={1}&courseId={2}&unitId={3}");

            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("9e983172-dba2-4ca7-b602-372c011ecb99"),
                column: "TemplateLink",
                value: "/learn/lesson/{0}?courseId={1}&unitId={2}&type=class-forum-comment&resultId={3}");

            migrationBuilder.InsertData(
                table: "NotificationTypes",
                columns: new[] { "Id", "Content", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "Icon", "IsDeleted", "Priority", "TemplateLink", "TemplateMessage", "Type", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[,]
                {
                    { new Guid("05441907-46a9-4178-9a69-fce295d670be"), "NoticePayment", new DateTime(2023, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, 1, "/pricing-plan", "Thời gian học thử của bạn sẽ kết thúc trong vòng 2 ngày tới. Bạn vui lòng gia hạn khóa học để đảm bảo tiếp tục hành trình học tập mà không bị gián đoạn nhé", "LinkPage", null, null, null },
                    { new Guid("27007edb-25d8-493e-84d8-85f6f9e165b2"), "DeleteComment", new DateTime(2023, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, 1, "/learn/lesson/{0}?courseId={1}&unitId={2}&type=class-forum-comment&resultId={3}", "Bình luận của bạn trong bài viết của {0} đã bị gỡ do vi phạm tiêu chuẩn cộng đồng của FSEL.", "LinkComment", null, null, null },
                    { new Guid("3881403a-d412-4593-aee5-5a38f4e078f9"), "OrderCreate", new DateTime(2023, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, 1, "/payment", "Bạn có hóa đơn khóa học mới phê duyệt. Nhấn để phê duyệt.", "Text", null, null, null },
                    { new Guid("46526095-5284-46f0-8c86-e00d95f1c985"), "FullMockTest", new DateTime(2023, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, 1, "/learn/full-mock-test/{0}?courseId={1}", "Bài {0} đã được giáo viên chấm điểm. Nhấn để xem chi tiết", "LinkPage", null, null, null },
                    { new Guid("57c709a6-03bd-4a3f-ad34-f9449587e1df"), "DeleteClassForumResult", new DateTime(2023, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, 1, "/learn/lesson/{0}?courseId={1}&unitId={2}&type=class-forum", "Bài viết của bạn trong {0} đã bị gỡ do vi phạm tiêu chuẩn cộng đồng của FSEL. Vui lòng thử lại!", "LinkPage", null, null, null },
                    { new Guid("69cc9e39-82d5-4d1a-81b9-8c97ec54bfad"), "ApprovePostClassForum", new DateTime(2023, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, 1, "/learn/lesson/{0}?courseId={1}&unitId={2}&type=class-forum", "Bài đăng của bạn đã được phê duyệt. Nhấn để xem chi tiết", "LinkPage", null, null, null },
                    { new Guid("75540141-a6d9-409d-a3ce-90c9a7577e8a"), "RejectApprovalPostClassForum", new DateTime(2023, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, 1, "/learn/lesson/{0}?courseId={1}&unitId={2}&type=class-forum", "Bài viết của bạn trong {0} đã bị từ chối phê duyệt do vi phạm tiêu chuẩn cộng đồng của FSEL. Vui lòng thử lại!", "LinkPage", null, null, null },
                    { new Guid("9da28757-2d28-40cb-b88c-cc51fc15bb43"), "LeaderBoard", new DateTime(2023, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, 1, "/home", "Chúc mừng bạn đã đạt top {0} trên Bảng xếp hạng!", "LinkPage", null, null, null },
                    { new Guid("a99bfaae-4139-4766-9405-ef24332fb51a"), "NoticeExpireAfterTwoDay", new DateTime(2023, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, 1, "/pricing-plan", "Khóa học của bạn sẽ kết thúc trong 2 ngày tới.Bạn vui lòng gia hạn khóa học để đảm bảo tiếp tục hành trình học tập mà không bị gián đoạn nhé", "LinkPage", null, null, null },
                    { new Guid("cd8a1edb-029f-4b2d-9725-c796e6fd7e5b"), "OrderChangeStatus", new DateTime(2023, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, 1, "/learn", "Bạn đã mua khóa học {0} thành công. Hãy bắt đầu học nào!", "Text", null, null, null },
                    { new Guid("e89b5850-33ff-4922-89e1-1a2f7de29375"), "CreateClassForumResult", new DateTime(2023, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, 1, "/class-forum-management", "Bạn có bài đăng của học sinh đang chờ duyệt.", "Text", null, null, null },
                    { new Guid("f9d5a75f-fd55-4b1e-ade8-a446b6465e34"), "NoticeExpireAfterTwoWeek", new DateTime(2023, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, 1, "/pricing-plan", "Khóa học của bạn sẽ kết thúc trong 2 tuần tới.Bạn vui lòng gia hạn khóa học để đảm bảo tiếp tục hành trình học tập mà không bị gián đoạn nhé", "LinkPage", null, null, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("05441907-46a9-4178-9a69-fce295d670be"));

            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("27007edb-25d8-493e-84d8-85f6f9e165b2"));

            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("3881403a-d412-4593-aee5-5a38f4e078f9"));

            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("46526095-5284-46f0-8c86-e00d95f1c985"));

            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("57c709a6-03bd-4a3f-ad34-f9449587e1df"));

            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("69cc9e39-82d5-4d1a-81b9-8c97ec54bfad"));

            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("75540141-a6d9-409d-a3ce-90c9a7577e8a"));

            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("9da28757-2d28-40cb-b88c-cc51fc15bb43"));

            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("a99bfaae-4139-4766-9405-ef24332fb51a"));

            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("cd8a1edb-029f-4b2d-9725-c796e6fd7e5b"));

            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("e89b5850-33ff-4922-89e1-1a2f7de29375"));

            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("f9d5a75f-fd55-4b1e-ade8-a446b6465e34"));

            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("1a7f9999-79e2-4eb1-85a8-cc8d1398f732"),
                column: "TemplateLink",
                value: "");

            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("59942527-ca1f-4f18-a66b-cdcfe87b317e"),
                column: "TemplateLink",
                value: "");

            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("810c70ca-4f3c-4d02-a194-708fa45d42ea"),
                column: "TemplateLink",
                value: "/learn/{0}?courseId={1}&unitId={0}&type=classForum&classForumResultId={2}&commentId={3}");

            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("810c70ca-4f3c-4d02-a491-708fa45d42ea"),
                column: "TemplateLink",
                value: "/learn/{0}?courseId={1}&unitId={0}&type=classForum&classForumResultId={2}");

            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("85062a12-ab5b-486c-a8ab-db329ea7f46d"),
                column: "TemplateLink",
                value: "");

            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("9e983172-dba2-4ca7-b602-372c011ecb99"),
                column: "TemplateLink",
                value: "/learn/{0}?courseId={1}&unitId={0}&type=classForum&classForumResultId={1}&commentId={2}&repCommentId={3}");

            migrationBuilder.InsertData(
                table: "NotificationTypes",
                columns: new[] { "Id", "Content", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "Icon", "IsDeleted", "Priority", "TemplateLink", "TemplateMessage", "Type", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[,]
                {
                    { new Guid("17c0df26-f131-42bf-b315-0a8a85524f6f"), "DeleteClassForumResult", new DateTime(2023, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, 1, "/learn/{0}?courseId={1}&unitId={0}&type=classForum&classForumResultId={2}", "Bài viết của bạn đã vi phạm Tiêu chuẩn cộng đồng của FSEL và đã bị xóa", "LinkPage", null, null, null },
                    { new Guid("86c0df96-f131-42bf-b315-0a8a85584f6a"), "OrderCreate", new DateTime(2023, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, 1, "/payment", "Bạn có hóa đơn khóa học mới phê duyệt. Nhấn để phê duyệt.", "Text", null, null, null },
                    { new Guid("96c0df96-f131-42bf-b315-0a8a85524f6f"), "DeleteComment", new DateTime(2023, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, 1, "/learn/{0}?courseId={1}&unitId={0}&type=classForum&classForumResultId={2}&commentId={3}", "Bình luận của bạn đã vi phạm Tiêu chuẩn cộng đồng của FSEL và đã bị xóa", "LinkComment", null, null, null },
                    { new Guid("a805f268-467b-4149-8d01-b2ea05e1f3bd"), "OrderChangeStatus", new DateTime(2023, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, 1, "/learn", "Bạn đã mua khóa học {0} thành công. Hãy bắt đầu học nào!", "Text", null, null, null },
                    { new Guid("d789788a-1ba6-405b-f280-ae92cd3b4fc2"), "CreateClassForumResult", new DateTime(2023, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, 1, "/class-forum-management", "Bạn có bài đăng của học sinh đang chờ duyệt.", "Text", null, null, null }
                });
        }
    }
}
