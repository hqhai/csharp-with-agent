using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Fsel.Notification.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_Notify_CourseGoal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "NotificationTypes",
                columns: new[] { "Id", "Content", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "Icon", "IsDeleted", "Priority", "TemplateLink", "TemplateMessage", "Type", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[,]
                {
                    { new Guid("3e2f86ad-f8de-4d33-b3a8-49bb2511ec73"), "AchievedCourseGoal", new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, 1, "", "Thời gian hoạt động trung bình trong tuần trước của bạn là {0} giờ {1} phút mỗi ngày. Số bài học đã hoàn thành: {2} (Mục tiêu: {3}). Tuyệt vời! Bạn đã hoàn thành mục tiêu của mình tuần này. Tiếp tục cố gắng nhé!", "LinkPage", null, null, null },
                    { new Guid("50b4314c-2c85-4f3a-82ad-c4f3e93947a1"), "CourseGoalStudent", new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, 1, "", "Tuần này, giáo viên của bạn đã đặt mục tiêu cho b{0}ạn là hoàn thành {0} bài học 📚. Hãy cố gắng hết mình để hoàn thành nhé! 💪", "LinkPage", null, null, null },
                    { new Guid("9bcbce1d-22a4-4b33-bf6c-53140a4a7732"), "ExceededCourseGoal", new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, 1, "", "Thời gian hoạt động trung bình tuần trước của bạn là {0} giờ {1} phút mỗi ngày. Số bài học đã hoàn thành: {2} (Mục tiêu: {3}). Xuất sắc! Bạn đã vượt qua mục tiêu tuần này. Tiếp tục phong độ ấn tượng này nhé!", "LinkPage", null, null, null },
                    { new Guid("b65e04cc-3d38-42a7-b7d1-8f66241f166a"), "BelowTargetCourseGoal", new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, 1, "", "Thời gian hoạt động trung bình trong tuần trước của bạn là {0} giờ {1} phút mỗi ngày. Số bài học đã hoàn thành: {2} (Mục tiêu: {3}). Bạn gần đạt rồi! Chỉ cần thêm chút nỗ lực nữa là hoàn thành mục tiêu thôi!", "LinkPage", null, null, null }
                });

            migrationBuilder.InsertData(
                table: "NotificationTypeTranslations",
                columns: new[] { "Id", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "IsDeleted", "Language", "NotificationTypeId", "TemplateMessage", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[,]
                {
                    { new Guid("36f0d5e9-67b9-4901-86ee-2f1e60b4b199"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "vi-VN", new Guid("9bcbce1d-22a4-4b33-bf6c-53140a4a7732"), "Thời gian hoạt động trung bình tuần trước của bạn là {0} giờ {1} phút mỗi ngày. Số bài học đã hoàn thành: {2} (Mục tiêu: {3}). Xuất sắc! Bạn đã vượt qua mục tiêu tuần này. Tiếp tục phong độ ấn tượng này nhé!", null, null, null },
                    { new Guid("4a5a8e72-adcf-4e5a-b95d-4b9fb028e5da"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "fr-FR", new Guid("50b4314c-2c85-4f3a-82ad-c4f3e93947a1"), "Cette semaine, votre enseignant(e) vous a fixé comme objectif de terminer {0} leçons 📚. Donnez le meilleur de vous-même pour y parvenir ! 💪", null, null, null },
                    { new Guid("a6fb1965-4f1a-4819-994d-3bea1183ada8"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "fr-FR", new Guid("b65e04cc-3d38-42a7-b7d1-8f66241f166a"), "Votre temps moyen d'activité la semaine dernière était de {0} heures et {1} minutes par jour. Leçons terminées : {2} (Objectif : {3}). Vous y êtes presque ! Avec un peu plus d’effort, vous atteindrez votre objectif !", null, null, null },
                    { new Guid("a88b0eb9-2d63-4373-94b1-1cf2a212ad89"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "vi-VN", new Guid("3e2f86ad-f8de-4d33-b3a8-49bb2511ec73"), "Thời gian hoạt động trung bình trong tuần trước của bạn là {0} giờ {1} phút mỗi ngày. Số bài học đã hoàn thành: {2} (Mục tiêu: {3}). Tuyệt vời! Bạn đã hoàn thành mục tiêu của mình tuần này. Tiếp tục cố gắng nhé!", null, null, null },
                    { new Guid("b7a4107e-f59f-48de-9c93-41d39a6c3a80"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "vi-VN", new Guid("b65e04cc-3d38-42a7-b7d1-8f66241f166a"), "Thời gian hoạt động trung bình trong tuần trước của bạn là {0} giờ {1} phút mỗi ngày. Số bài học đã hoàn thành: {2} (Mục tiêu: {3}). Bạn gần đạt rồi! Chỉ cần thêm chút nỗ lực nữa là hoàn thành mục tiêu thôi!", null, null, null },
                    { new Guid("b8e6d8d5-2e72-4ecf-bf0a-fccaf989c8c2"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "vi-VN", new Guid("50b4314c-2c85-4f3a-82ad-c4f3e93947a1"), "Tuần này, giáo viên của bạn đã đặt mục tiêu cho bạn là hoàn thành {0} bài học 📚. Hãy cố gắng hết mình để hoàn thành nhé! 💪", null, null, null },
                    { new Guid("b9984fcb-f6e1-4899-97a2-0fc539b9a8d1"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "en-US", new Guid("9bcbce1d-22a4-4b33-bf6c-53140a4a7732"), "Your average activity time last week was {0} hours and {1} minutes per day. Lessons completed: {2} (Goal: {3}). Excellent! You have exceeded your goal for the week. Keep up the impressive performance!", null, null, null },
                    { new Guid("c89f46a5-c352-4668-bf28-166cb49ee05e"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "en-US", new Guid("b65e04cc-3d38-42a7-b7d1-8f66241f166a"), "Your average activity time last week was {0} hours and {1} minutes per day. Lessons completed: {2} (Goal: {3}). You're almost there! Just a bit more effort, and you'll reach your goal!", null, null, null },
                    { new Guid("e70f4a44-3bb5-4b9a-bcc1-5f7d5f9f0b0b"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "en-US", new Guid("50b4314c-2c85-4f3a-82ad-c4f3e93947a1"), "This week, your teacher has set a goal for you to complete {0} lessons 📚. Give it your best effort to accomplish it! 💪", null, null, null },
                    { new Guid("f32f74a1-1b8b-4a13-970b-b0327e5cb839"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "en-US", new Guid("3e2f86ad-f8de-4d33-b3a8-49bb2511ec73"), "Your average activity time last week was {0} hours and {1} minutes per day. Lessons completed: {2} (Goal: {3}). Great job! You have achieved your goal this week. Keep up the good work!", null, null, null },
                    { new Guid("f402c9fb-3581-439d-b530-e6b2c89df125"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "fr-FR", new Guid("3e2f86ad-f8de-4d33-b3a8-49bb2511ec73"), "Votre temps moyen d'activité la semaine dernière était de {0} heures et {1} minutes par jour. Leçons terminées : {2} (Objectif : {3}). Excellent travail ! Vous avez atteint votre objectif cette semaine. Continuez ainsi !", null, null, null },
                    { new Guid("fc4570c8-6e40-4b5a-b9d6-9f1e21aac7b5"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "fr-FR", new Guid("9bcbce1d-22a4-4b33-bf6c-53140a4a7732"), "Votre temps moyen d'activité la semaine dernière était de {0} heures et {1} minutes par jour. Leçons terminées : {2} (Objectif : {3}). Excellent ! Vous avez dépassé votre objectif de la semaine. Continuez sur cette belle lancée !", null, null, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("36f0d5e9-67b9-4901-86ee-2f1e60b4b199"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("4a5a8e72-adcf-4e5a-b95d-4b9fb028e5da"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("a6fb1965-4f1a-4819-994d-3bea1183ada8"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("a88b0eb9-2d63-4373-94b1-1cf2a212ad89"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("b7a4107e-f59f-48de-9c93-41d39a6c3a80"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("b8e6d8d5-2e72-4ecf-bf0a-fccaf989c8c2"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("b9984fcb-f6e1-4899-97a2-0fc539b9a8d1"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("c89f46a5-c352-4668-bf28-166cb49ee05e"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("e70f4a44-3bb5-4b9a-bcc1-5f7d5f9f0b0b"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("f32f74a1-1b8b-4a13-970b-b0327e5cb839"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("f402c9fb-3581-439d-b530-e6b2c89df125"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("fc4570c8-6e40-4b5a-b9d6-9f1e21aac7b5"));

            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("3e2f86ad-f8de-4d33-b3a8-49bb2511ec73"));

            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("50b4314c-2c85-4f3a-82ad-c4f3e93947a1"));

            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("9bcbce1d-22a4-4b33-bf6c-53140a4a7732"));

            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("b65e04cc-3d38-42a7-b7d1-8f66241f166a"));
        }
    }
}
