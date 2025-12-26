using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Fsel.Notification.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_NotificationType_Add_Notify_AfterDonePT : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("0b16fc25-ec1d-4677-84a3-6761afa2bb03"),
                column: "Content",
                value: "Day3At7h30AfterChooseLevel");

            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("1c23bddb-426a-4749-95ec-ddcbad71af9e"),
                column: "Content",
                value: "Day1At7h30AfterChooseLevel");

            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("23990ecc-834f-44b7-a7a5-a03baa703b32"),
                column: "Content",
                value: "Day7At19h30AfterChooseLevel");

            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("29fd645f-4e60-4c0a-8266-d24c9854310e"),
                column: "Content",
                value: "OneHourAfterChooseLevel");

            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("2d4af96c-5344-42f3-b31f-584fa05adee2"),
                column: "Content",
                value: "Day3At12h00AfterChooseLevel");

            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("3df9fae4-6879-4196-b70e-4776b65fc51a"),
                column: "Content",
                value: "Day2At12h00AfterChooseLevel");

            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("5d324b4a-d608-4e49-9992-b6703c534550"),
                column: "Content",
                value: "Day2At19h30AfterChooseLevel");

            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("8607c5e0-15e4-4a52-9312-ead10428f8c8"),
                column: "Content",
                value: "Day5At12h00AfterChooseLevel");

            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("921cdf08-0201-4a3f-a191-d415c4101018"),
                column: "Content",
                value: "Day4At17h30AfterChooseLevel");

            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("b2de4124-4b28-4120-8156-ddef65cde6bd"),
                column: "Content",
                value: "Day1At19h30AfterChooseLevel");

            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("c15a5dd2-c897-464f-9f1c-5aa8db6cae40"),
                column: "Content",
                value: "Day6At19h30AfterChooseLevel");

            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("c182d087-fe4f-413c-ae1c-0042899e5b16"),
                column: "Content",
                value: "Day7At7h30AfterChooseLevel");

            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("c2a40d5d-6735-40cb-a090-193aa93bcca2"),
                column: "Content",
                value: "Day6At12h00AfterChooseLevel");

            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("e10e8e9e-a132-41f2-b9e8-5a6ec055be2e"),
                column: "Content",
                value: "Day6At17h30AfterChooseLevel");

            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("f398551d-cd3a-4641-bc82-473d718df074"),
                column: "Content",
                value: "Day7At12h00AfterChooseLevel");

            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("fe46415d-4e6e-4248-b828-785f5486b41f"),
                column: "Content",
                value: "Day7At17h30AfterChooseLevel");

            migrationBuilder.InsertData(
                table: "NotificationTypes",
                columns: new[] { "Id", "Content", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "Icon", "IsDeleted", "Priority", "TemplateLink", "TemplateMessage", "Type", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[,]
                {
                    { new Guid("02111ed0-9c34-4c08-ad52-896f843622bb"), "Day3At7h30AfterDonePT", new DateTime(2025, 6, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, 1, "/change-level", "🌱 Mọi hành trình đều bắt đầu từ bước đầu tiên Chọn khóa học đầu tiên để FSEL đồng hành cùng bạn mỗi ngày!", "Text", null, null, null },
                    { new Guid("19876859-11c6-4d5d-a043-6325af55817f"), "Day5At12h00AfterDonePT", new DateTime(2025, 6, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, 1, "/change-level", "💰 Techie: “Nhiệm vụ hôm nay – tích thêm 100 coin!” Học bài mới = coin mới. Gom xu mỗi ngày để đổi quà cực chất trong Tháng Tự Học!", "Text", null, null, null },
                    { new Guid("19c331f9-7aed-4c21-a7a6-dd4b9e90a285"), "Day3At17h30AfterDonePT", new DateTime(2025, 6, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, 1, "/change-level", "📚 Lộ trình học của bạn đang chờ! FSEL đã gợi ý khóa học phù hợp – vào chọn ngay để không lỡ nhịp cùng bạn bè!", "Text", null, null, null },
                    { new Guid("2655aa79-8a88-4a1f-9449-1884cb791123"), "OneHourAfterDonePT", new DateTime(2025, 6, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, 1, "/change-level", "👉 Nhận ngay 160 FSEL Coin thôi nào! 🎯 Bạn đã hoàn thành bài kiểm tra! Lộ trình học cá nhân đã sẵn sàng. Nhấn để bắt đầu săn FSEL Coin ngay 💪", "Text", null, null, null },
                    { new Guid("56d463aa-463a-4b91-96d9-b50cf16720b9"), "Day3At12h00AfterDonePT", new DateTime(2025, 6, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, 1, "/change-level", "⚠️ Techie cảnh báo: Bạn đang bị tụt lại phía sau! Mỗi ngày học = coin + cơ hội trúng Túi Mù FSEL! Không học = không quà 😢", "Text", null, null, null },
                    { new Guid("5b08de2c-0de4-4287-a07c-b7def40313e9"), "Day7At7h30AfterDonePT", new DateTime(2025, 6, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, 1, "/change-level", "⏳ Còn 2 ngày để nhận quà khởi động! Nếu bạn chưa chọn khóa học, coin thưởng sẽ hết hạn. Tối nay vào học ngay nhé!", "Text", null, null, null },
                    { new Guid("62413604-f229-4e39-aa96-3b0a3acad3e3"), "Day7At19h30AfterDonePT", new DateTime(2025, 6, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, 1, "/change-level", "😢 Techie: “Tớ có nói gì đâu, bạn đã học đâu mà bỏ rồi?” Bài học đầu đang đợi bạn. Không vào là coin mất, Techie buồn!", "Text", null, null, null },
                    { new Guid("63685b29-a8a4-4117-b4ca-dd8e08b9e439"), "Day6At17h30AfterDonePT", new DateTime(2025, 6, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, 1, "/change-level", "📊 AI dự đoán bạn sẽ học bài hôm nay …Nếu đúng, Techie sẽ thưởng bạn 160 coin. Nếu sai… Techie sẽ buồn thêm lần nữa 😞", "Text", null, null, null },
                    { new Guid("65bc348b-99a8-4573-af05-7d4108fc11d2"), "Day5At19h30AfterDonePT", new DateTime(2025, 6, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, 1, "/change-level", "Cơm thì ăn đều, mà bài thì bỏ đó? 😅 Techie thấy bạn có tiềm năng đổi iPhone nếu học đều đó. Vào học lẹ đi người đẹp!", "Text", null, null, null },
                    { new Guid("85fe84d6-d116-4b69-86f8-f97849888f87"), "Day1At19h30AfterDonePT", new DateTime(2025, 6, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, 1, "/change-level", "🚀 Bắt đầu hành trình học tiếng Anh của bạn ngay hôm nay! Đừng để tài khoản nằm yên – chọn khóa học đầu tiên và khám phá FSEL Store!", "Text", null, null, null },
                    { new Guid("91b551e5-d4f1-4aee-bf08-582678cf5250"), "Day2At12h00AfterDonePT", new DateTime(2025, 6, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, 1, "/change-level", "⏰ Còn vài giờ để nhận thưởng! Bài học đầu tiên giúp bạn mở khóa coin và cơ hội bóc Túi Mù FSEL – đừng bỏ lỡ ⏳", "Text", null, null, null },
                    { new Guid("ba4ccb0d-ee54-46b6-b091-e6c99ee1f224"), "Day1At7h30AfterDonePT", new DateTime(2025, 6, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, 1, "/change-level", "🎁 Chọn khóa học – nhận ngay phần quà khởi động! Mỗi khóa học là một cơ hội nhận coin, leo bảng thi đua và đổi quà Túi Mù FSEL!", "Text", null, null, null },
                    { new Guid("cc34c9bf-e7e1-48db-bd33-c9351aaa7c5f"), "Day6At12h00AfterDonePT", new DateTime(2025, 6, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, 1, "/change-level", "😮 Lớp bạn đã có 12 người học rồi, còn bạn thì sao? 🤖 Techie nhắc bạn: càng học sớm, càng dễ gom coin và lên bảng vàng!", "Text", null, null, null },
                    { new Guid("ce9bea9f-dcf9-4a62-8580-218889016932"), "Day4At17h30AfterDonePT", new DateTime(2025, 6, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, 1, "/change-level", "⏱️ Cảnh báo từ Techie: Kho quà sắp... tan biến! Quà tại FSEL Store chỉ còn trong vài vòng quay nữa. Vào học để gom coin liền tay!", "Text", null, null, null },
                    { new Guid("df161229-9390-492b-85b0-b2d0fc61cc89"), "Day4At7h30AfterDonePT", new DateTime(2025, 6, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, 1, "/change-level", "👨‍🚀 Trung tâm chỉ huy đang chờ bạn phản hồi! 🤖 Techie đã định vị lộ trình. Hãy vào học bài đầu để khởi động chuyến bay!", "Text", null, null, null },
                    { new Guid("f2f234d2-abab-4bef-8e05-be83776628a8"), "Day2At19h30AfterDonePT", new DateTime(2025, 6, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, 1, "/change-level", "😱 U là trời! Bạn bỏ lỡ 2 ngày học rồi đó… Coin không học là mất, quà không tích là bay. Techie cảnh báo bạn đang trôi khỏi bảng vàng!", "Text", null, null, null },
                    { new Guid("f30517dd-af21-499c-88a2-46a22ad142a3"), "Day7At17h30AfterDonePT", new DateTime(2025, 6, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, 1, "/change-level", "⏳ Đừng để hôm nay trôi qua mà chưa chọn khóa học! Học sớm hơn = nhận coin sớm hơn! Bắt đầu từ bài đầu tiên ngay nhé!", "Text", null, null, null },
                    { new Guid("f5a60340-dbb6-4f50-ad84-abaa77905bb2"), "Day6At19h30AfterDonePT", new DateTime(2025, 6, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, 1, "/change-level", "🍚 Cơm nước chưa người đẹp? Nếu ăn xong rồi thì… học 1 bài thôi nè. Coin đang đợi để được gom về túi bạn đó!", "Text", null, null, null },
                    { new Guid("fd440834-703d-4728-a059-bebcd17069d9"), "Day7At12h00AfterDonePT", new DateTime(2025, 6, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, 1, "/change-level", "💅 Học xong rồi lên Store đổi quà là slay hết nước chấm luôn á! Học 1 bài thôi, đổi tai nghe, loa, sách vở đủ cả. Vào học đi bạn slay!", "Text", null, null, null }
                });

            migrationBuilder.InsertData(
                table: "NotificationTypeTranslations",
                columns: new[] { "Id", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "IsDeleted", "Language", "NotificationTypeId", "TemplateMessage", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[,]
                {
                    { new Guid("044915bf-f8d7-4d10-b0f9-e39d8a4421a5"), new DateTime(2025, 6, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "fr-FR", new Guid("cc34c9bf-e7e1-48db-bd33-c9351aaa7c5f"), "😮 Votre classe compte déjà 12 étudiants, et vous ? 🤖 Techie vous rappelle : plus vous commencez tôt, plus il est facile de collecter des pièces et de grimper dans le classement !", null, null, null },
                    { new Guid("0547e4ca-a9f4-4dcc-97e5-42d2bc4e7978"), new DateTime(2025, 6, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "en-US", new Guid("63685b29-a8a4-4117-b4ca-dd8e08b9e439"), "📊 AI predicts you'll study today… If correct, Techie will reward you with 160 coins. If wrong... Techie will be sad again 😞", null, null, null },
                    { new Guid("064f51ff-75af-4776-ae1b-4dbeb4bf4021"), new DateTime(2025, 6, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "fr-FR", new Guid("85fe84d6-d116-4b69-86f8-f97849888f87"), "🚀 Commencez votre parcours d’apprentissage de l’anglais dès aujourd’hui ! Ne laissez pas votre compte inactif – choisissez votre premier cours et explorez la boutique FSEL !", null, null, null },
                    { new Guid("076c6072-0d26-4065-a74f-b774f83c67f2"), new DateTime(2025, 6, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "vi-VN", new Guid("fd440834-703d-4728-a059-bebcd17069d9"), "💅 Học xong rồi lên Store đổi quà là slay hết nước chấm luôn á! Học 1 bài thôi, đổi tai nghe, loa, sách vở đủ cả. Vào học đi bạn slay!", null, null, null },
                    { new Guid("0900d63d-499c-43fc-b159-d69a4b134b22"), new DateTime(2025, 6, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "vi-VN", new Guid("56d463aa-463a-4b91-96d9-b50cf16720b9"), "⚠️ Techie cảnh báo: Bạn đang bị tụt lại phía sau! Mỗi ngày học = coin + cơ hội trúng Túi Mù FSEL! Không học = không quà 😢", null, null, null },
                    { new Guid("0ce416be-4e34-4efb-89e5-87eb592f48f7"), new DateTime(2025, 6, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "vi-VN", new Guid("df161229-9390-492b-85b0-b2d0fc61cc89"), "👨‍🚀 Trung tâm chỉ huy đang chờ bạn phản hồi! 🤖 Techie đã định vị lộ trình. Hãy vào học bài đầu để khởi động chuyến bay!", null, null, null },
                    { new Guid("0d787987-619a-47c6-a4a8-971e44cd5ae4"), new DateTime(2025, 6, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "vi-VN", new Guid("cc34c9bf-e7e1-48db-bd33-c9351aaa7c5f"), "😮 Lớp bạn đã có 12 người học rồi, còn bạn thì sao? 🤖 Techie nhắc bạn: càng học sớm, càng dễ gom coin và lên bảng vàng!", null, null, null },
                    { new Guid("13787811-66ed-4fdd-8f27-5a059d9021d4"), new DateTime(2025, 6, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "vi-VN", new Guid("ba4ccb0d-ee54-46b6-b091-e6c99ee1f224"), "🎁 Chọn khóa học – nhận ngay phần quà khởi động! Mỗi khóa học là một cơ hội nhận coin, leo bảng thi đua và đổi quà Túi Mù FSEL!", null, null, null },
                    { new Guid("28f5127e-d440-46ee-af42-00762465518c"), new DateTime(2025, 6, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "en-US", new Guid("85fe84d6-d116-4b69-86f8-f97849888f87"), "🚀 Start your English learning journey today! Don’t let your account sit idle – pick your first course and explore the FSEL Store!", null, null, null },
                    { new Guid("2d96bb55-f125-4445-909d-f15b73e4dff1"), new DateTime(2025, 6, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "en-US", new Guid("56d463aa-463a-4b91-96d9-b50cf16720b9"), "⚠️ Techie warns: You're falling behind! Every day you study = coin + chance to win FSEL Mystery Bag! No study = no reward 😢", null, null, null },
                    { new Guid("2e97428c-fa50-47c9-ad97-29bd3ea3b246"), new DateTime(2025, 6, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "fr-FR", new Guid("2655aa79-8a88-4a1f-9449-1884cb791123"), "👉 Obtenez 160 pièces FSEL maintenant ! 🎯 Vous avez terminé le test ! Le parcours d’apprentissage personnel est prêt. Cliquez pour commencer à chasser les pièces FSEL maintenant 💪", null, null, null },
                    { new Guid("3093e36a-8e21-43c2-92a1-6b62f8e3045e"), new DateTime(2025, 6, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "fr-FR", new Guid("19876859-11c6-4d5d-a043-6325af55817f"), "💰 Techie : \"La mission du jour – gagnez 100 pièces !\" Apprenez une nouvelle leçon = nouvelles pièces. Collectez des pièces chaque jour pour échanger contre de superbes récompenses pendant le Mois d'Auto-apprentissage !", null, null, null },
                    { new Guid("321c1cee-f949-46f0-bbc8-940f2a5881f9"), new DateTime(2025, 6, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "vi-VN", new Guid("f30517dd-af21-499c-88a2-46a22ad142a3"), "⏳ Đừng để hôm nay trôi qua mà chưa chọn khóa học! Học sớm hơn = nhận coin sớm hơn! Bắt đầu từ bài đầu tiên ngay nhé!", null, null, null },
                    { new Guid("33ed40bd-5069-41d2-a9e0-ffd354ae8097"), new DateTime(2025, 6, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "fr-FR", new Guid("19c331f9-7aed-4c21-a7a6-dd4b9e90a285"), "📚 Votre parcours d'apprentissage vous attend ! FSEL a suggéré un cours adapté – choisissez maintenant pour ne pas prendre de retard par rapport à vos amis !", null, null, null },
                    { new Guid("346c2afd-3ee8-4b54-8c8e-1bb1399a5ced"), new DateTime(2025, 6, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "vi-VN", new Guid("19c331f9-7aed-4c21-a7a6-dd4b9e90a285"), "📚 Lộ trình học của bạn đang chờ! FSEL đã gợi ý khóa học phù hợp – vào chọn ngay để không lỡ nhịp cùng bạn bè!", null, null, null },
                    { new Guid("39a928d0-1610-41e8-8357-3655c1dedc2b"), new DateTime(2025, 6, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "en-US", new Guid("19c331f9-7aed-4c21-a7a6-dd4b9e90a285"), "📚 Your learning path is waiting! FSEL has suggested a suitable course – choose now so you don’t fall behind your friends!", null, null, null },
                    { new Guid("3ea49bca-b0eb-4fa0-8b58-79a06e9b1d96"), new DateTime(2025, 6, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "vi-VN", new Guid("62413604-f229-4e39-aa96-3b0a3acad3e3"), "😢 Techie: \"Tớ có nói gì đâu, bạn đã học đâu mà bỏ rồi?\" Bài học đầu đang đợi bạn. Không vào là coin mất, Techie buồn!", null, null, null },
                    { new Guid("43d69e76-fc50-4204-b351-c23157540f28"), new DateTime(2025, 6, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "en-US", new Guid("f5a60340-dbb6-4f50-ad84-abaa77905bb2"), "Have you eaten yet, beautiful? If you’re done, then… just study one lesson. Coins are waiting to be collected in your pocket!", null, null, null },
                    { new Guid("4553653f-3416-43c1-8efc-47657e7ad0ba"), new DateTime(2025, 6, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "vi-VN", new Guid("85fe84d6-d116-4b69-86f8-f97849888f87"), "🚀 Bắt đầu hành trình học tiếng Anh của bạn ngay hôm nay! Đừng để tài khoản nằm yên – chọn khóa học đầu tiên và khám phá FSEL Store!", null, null, null },
                    { new Guid("45734f25-0754-451a-83a8-f2c27ec2dd77"), new DateTime(2025, 6, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "en-US", new Guid("02111ed0-9c34-4c08-ad52-896f843622bb"), "🌱 Every journey starts with a first step. Choose your first course and let FSEL accompany you every day!", null, null, null },
                    { new Guid("494fc24e-d137-4298-805f-6140e990124d"), new DateTime(2025, 6, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "fr-FR", new Guid("f30517dd-af21-499c-88a2-46a22ad142a3"), "⏳ Ne laissez pas passer aujourd'hui sans avoir choisi un cours ! Étudier plus tôt = gagner des pièces plus tôt ! Commencez dès maintenant par la première leçon !", null, null, null },
                    { new Guid("4be1d8fd-02f6-46b3-b6c5-96ec75d138af"), new DateTime(2025, 6, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "en-US", new Guid("cc34c9bf-e7e1-48db-bd33-c9351aaa7c5f"), "😮 Your class already has 12 students, how about you? 🤖 Techie reminds you: the earlier you start studying, the easier it is to collect coins and climb the leaderboard!", null, null, null },
                    { new Guid("4f6508d1-835b-4378-b5fd-d74ce6b249b9"), new DateTime(2025, 6, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "fr-FR", new Guid("62413604-f229-4e39-aa96-3b0a3acad3e3"), "😢 Techie : \"Je n'ai rien dit, mais tu n'as pas encore étudié, pourquoi partir ?\" La première leçon t'attend. Si tu ne rejoins pas, les pièces sont perdues, et Techie est triste !", null, null, null },
                    { new Guid("51c2bc5d-641e-4b77-95fa-2199b95c1198"), new DateTime(2025, 6, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "vi-VN", new Guid("f2f234d2-abab-4bef-8e05-be83776628a8"), "😱 U là trời! Bạn bỏ lỡ 2 ngày học rồi đó… Coin không học là mất, quà không tích là bay. Techie cảnh báo bạn đang trôi khỏi bảng vàng!", null, null, null },
                    { new Guid("59c32b91-ca6f-4312-af18-6dfa9e4c78a1"), new DateTime(2025, 6, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "fr-FR", new Guid("91b551e5-d4f1-4aee-bf08-582678cf5250"), "⏰ Il ne vous reste que quelques heures pour réclamer votre récompense ! La première leçon débloque des pièces et une chance d’ouvrir un sac mystère FSEL – ne manquez pas cette occasion ⏳", null, null, null },
                    { new Guid("6402dea8-dff7-461e-9e87-e69097a19819"), new DateTime(2025, 6, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "en-US", new Guid("65bc348b-99a8-4573-af05-7d4108fc11d2"), "You eat regularly, but leave your lessons undone? 😅 Techie sees you have the potential to trade for an iPhone if you study regularly. Start studying now, beautiful!", null, null, null },
                    { new Guid("6c45046e-2eea-40d8-a207-4e8c8d813a85"), new DateTime(2025, 6, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "en-US", new Guid("ce9bea9f-dcf9-4a62-8580-218889016932"), "⏱️ Warning from Techie: The gift stash is about to... vanish! FSEL Store rewards are running out in just a few spins. Study now to collect coins quickly!", null, null, null },
                    { new Guid("6d1d4f8b-aa22-4165-8526-de2cf552037a"), new DateTime(2025, 6, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "en-US", new Guid("f2f234d2-abab-4bef-8e05-be83776628a8"), "😱 Oh no! You’ve missed 2 days of learning… No study, no coins. No progress, no gifts. Techie warns you’re slipping off the leaderboard!", null, null, null },
                    { new Guid("6deaa803-86b7-42aa-b849-20a2ee13760b"), new DateTime(2025, 6, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "fr-FR", new Guid("56d463aa-463a-4b91-96d9-b50cf16720b9"), "⚠️ Techie avertit : Vous prenez du retard ! Chaque jour d'étude = coin + chance de gagner un Sac Mystère FSEL ! Pas d'étude = pas de récompense 😢", null, null, null },
                    { new Guid("6f84789f-ee86-4196-8e9a-d6211bd8bf72"), new DateTime(2025, 6, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "vi-VN", new Guid("ce9bea9f-dcf9-4a62-8580-218889016932"), "⏱️ Cảnh báo từ Techie: Kho quà sắp... tan biến! Quà tại FSEL Store chỉ còn trong vài vòng quay nữa. Vào học để gom coin liền tay!", null, null, null },
                    { new Guid("70991cb8-d0bc-4471-b9c1-4947a93d9f2d"), new DateTime(2025, 6, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "en-US", new Guid("2655aa79-8a88-4a1f-9449-1884cb791123"), "👉 Get 160 FSEL Coins now! 🎯 You have completed the test! Your personal learning path is ready. Click to start hunting for FSEL Coins now 💪", null, null, null },
                    { new Guid("77b6a9a9-3c1b-4033-9cd6-9cb60a664230"), new DateTime(2025, 6, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "vi-VN", new Guid("65bc348b-99a8-4573-af05-7d4108fc11d2"), "Cơm thì ăn đều, mà bài thì bỏ đó? 😅 Techie thấy bạn có tiềm năng đổi iPhone nếu học đều đó. Vào học lẹ đi người đẹp!", null, null, null },
                    { new Guid("7a2e0d42-7928-4aa7-b997-6271ea70d48b"), new DateTime(2025, 6, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "fr-FR", new Guid("df161229-9390-492b-85b0-b2d0fc61cc89"), "👨‍🚀 Le centre de commande attend votre réponse ! 🤖 Techie a défini le parcours d'apprentissage. Commencez votre voyage en prenant la première leçon !", null, null, null },
                    { new Guid("7de5fc5a-b421-4292-b6f7-e740e0551df8"), new DateTime(2025, 6, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "fr-FR", new Guid("63685b29-a8a4-4117-b4ca-dd8e08b9e439"), "📊 L'IA prévoit que vous allez étudier aujourd'hui... Si c'est vrai, Techie vous récompensera avec 160 pièces. Si c'est faux... Techie sera encore triste 😞", null, null, null },
                    { new Guid("82e71cbc-53ed-4dfe-bc05-bd038f14efa0"), new DateTime(2025, 6, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "en-US", new Guid("fd440834-703d-4728-a059-bebcd17069d9"), "💅 Study, then head to the store to redeem gifts – you’ll be slaying! Study just one lesson, and you can get headphones, speakers, books, and more. Go study, you’re slaying!", null, null, null },
                    { new Guid("83bb070c-ef7b-43a6-9401-778afe112066"), new DateTime(2025, 6, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "fr-FR", new Guid("5b08de2c-0de4-4287-a07c-b7def40313e9"), "⏳ Il ne vous reste plus que 2 jours pour réclamer votre cadeau de départ ! Si vous n'avez pas encore choisi un cours, vos pièces de récompense expireront. Commencez à étudier ce soir !", null, null, null },
                    { new Guid("8ea67da6-9f4d-4def-8f5b-ff03c6476e98"), new DateTime(2025, 6, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "fr-FR", new Guid("f2f234d2-abab-4bef-8e05-be83776628a8"), "😱 Mon dieu ! Vous avez manqué 2 jours d’étude… Pas d’apprentissage, pas de pièces. Pas de progrès, pas de cadeaux. Techie vous prévient : vous quittez le tableau d’honneur !", null, null, null },
                    { new Guid("8f847b4f-7843-4b7a-902e-683342d3e0f9"), new DateTime(2025, 6, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "fr-FR", new Guid("02111ed0-9c34-4c08-ad52-896f843622bb"), "🌱 Chaque voyage commence par un premier pas. Choisissez votre premier cours et laissez FSEL vous accompagner chaque jour !", null, null, null },
                    { new Guid("97678b67-48ce-40eb-8bc3-b2be51f7809d"), new DateTime(2025, 6, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "vi-VN", new Guid("5b08de2c-0de4-4287-a07c-b7def40313e9"), "⏳ Còn 2 ngày để nhận quà khởi động! Nếu bạn chưa chọn khóa học, coin thưởng sẽ hết hạn. Tối nay vào học ngay nhé!", null, null, null },
                    { new Guid("a2ca820a-c1ce-4bc9-b654-f833e5e9588a"), new DateTime(2025, 6, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "vi-VN", new Guid("91b551e5-d4f1-4aee-bf08-582678cf5250"), "⏰ Còn vài giờ để nhận thưởng! Bài học đầu tiên giúp bạn mở khóa coin và cơ hội bóc Túi Mù FSEL – đừng bỏ lỡ ⏳", null, null, null },
                    { new Guid("aaa34ec6-39da-4345-bb1b-01f71c7b2896"), new DateTime(2025, 6, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "vi-VN", new Guid("02111ed0-9c34-4c08-ad52-896f843622bb"), "🌱 Mọi hành trình đều bắt đầu từ bước đầu tiên Chọn khóa học đầu tiên để FSEL đồng hành cùng bạn mỗi ngày!", null, null, null },
                    { new Guid("aaa5368f-eec1-449f-8db4-89a317e3a234"), new DateTime(2025, 6, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "fr-FR", new Guid("ce9bea9f-dcf9-4a62-8580-218889016932"), "⏱️ Avertissement de Techie : Le stock de cadeaux est sur le point de... disparaître ! Les récompenses du FSEL Store ne seront plus disponibles après quelques tours. Étudiez maintenant pour collecter des pièces rapidement !", null, null, null },
                    { new Guid("adafdf21-a4fb-4035-bf71-39139bb4fc12"), new DateTime(2025, 6, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "vi-VN", new Guid("63685b29-a8a4-4117-b4ca-dd8e08b9e439"), "📊 AI dự đoán bạn sẽ học bài hôm nay …Nếu đúng, Techie sẽ thưởng bạn 160 coin. Nếu sai… Techie sẽ buồn thêm lần nữa 😞", null, null, null },
                    { new Guid("b8aba20e-47b3-4911-b5d0-0df5f62da9ed"), new DateTime(2025, 6, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "vi-VN", new Guid("2655aa79-8a88-4a1f-9449-1884cb791123"), "👉 Nhận ngay 160 FSEL Coin thôi nào! 🎯 Bạn đã hoàn thành bài kiểm tra! Lộ trình học cá nhân đã sẵn sàng. Nhấn để bắt đầu săn FSEL Coin ngay 💪", null, null, null },
                    { new Guid("b95dd6ad-c662-4077-bde7-3f723429aacf"), new DateTime(2025, 6, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "vi-VN", new Guid("f5a60340-dbb6-4f50-ad84-abaa77905bb2"), "🍚 Cơm nước chưa người đẹp? Nếu ăn xong rồi thì… học 1 bài thôi nè. Coin đang đợi để được gom về túi bạn đó!", null, null, null },
                    { new Guid("bdf8a59a-2bea-4000-bf2e-7584ff9153a9"), new DateTime(2025, 6, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "fr-FR", new Guid("fd440834-703d-4728-a059-bebcd17069d9"), "💅 Étudiez, puis allez au magasin pour échanger contre des cadeaux – vous allez tout déchirer ! Étudiez juste une leçon, et vous pourrez obtenir des écouteurs, des haut-parleurs, des livres et plus encore. Allez étudier, vous déchirez !", null, null, null },
                    { new Guid("bdfb081e-6b75-452b-a927-b9f2add14d06"), new DateTime(2025, 6, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "en-US", new Guid("62413604-f229-4e39-aa96-3b0a3acad3e3"), "😢 Techie: \"I didn’t say anything, but you haven’t studied yet, why did you leave?\" The first lesson is waiting for you. If you don’t join, the coins are lost, and Techie is sad!", null, null, null },
                    { new Guid("c48bc1b8-9ed3-4aae-ae3b-207820ed4e78"), new DateTime(2025, 6, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "en-US", new Guid("df161229-9390-492b-85b0-b2d0fc61cc89"), "👨‍🚀 The command center is waiting for your response! 🤖 Techie has set the learning path. Start your journey by taking the first lesson!", null, null, null },
                    { new Guid("d7ade9c0-5444-4584-acdc-e01ebfc6b69c"), new DateTime(2025, 6, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "en-US", new Guid("91b551e5-d4f1-4aee-bf08-582678cf5250"), "⏰ Only a few hours left to claim your reward! The first lesson unlocks coins and a chance to open a FSEL Mystery Bag – don’t miss out ⏳", null, null, null },
                    { new Guid("d8fd8423-0d0c-47ab-a20c-025c1c6252d0"), new DateTime(2025, 6, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "en-US", new Guid("f30517dd-af21-499c-88a2-46a22ad142a3"), "⏳ Don’t let today pass without choosing a course! Study earlier = earn coins earlier! Start from the first lesson now!", null, null, null },
                    { new Guid("dd392bfe-9379-43b3-b4b6-e8e0edefa2f5"), new DateTime(2025, 6, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "vi-VN", new Guid("19876859-11c6-4d5d-a043-6325af55817f"), "💰 Techie: \"Nhiệm vụ hôm nay – tích thêm 100 coin!\" Học bài mới = coin mới. Gom xu mỗi ngày để đổi quà cực chất trong Tháng Tự Học!", null, null, null },
                    { new Guid("f975284d-589e-4604-a230-e0d2e294557a"), new DateTime(2025, 6, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "en-US", new Guid("5b08de2c-0de4-4287-a07c-b7def40313e9"), "⏳ Only 2 days left to claim your starter gift! If you haven’t chosen a course yet, your reward coins will expire. Start studying tonight!", null, null, null },
                    { new Guid("f985110b-33f6-4282-9217-4d2a93c2a9b4"), new DateTime(2025, 6, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "en-US", new Guid("ba4ccb0d-ee54-46b6-b091-e6c99ee1f224"), "🎁 Choose a course – get a welcome gift right away! Each course is a chance to earn coins, climb the leaderboard, and exchange for a FSEL Mystery Bag!", null, null, null },
                    { new Guid("fa3b6ea3-6979-4e9d-b752-f3411fac9214"), new DateTime(2025, 6, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "fr-FR", new Guid("65bc348b-99a8-4573-af05-7d4108fc11d2"), "Vous mangez régulièrement, mais laissez vos leçons de côté ? 😅 Techie voit que vous avez le potentiel de gagner un iPhone si vous étudiez régulièrement. Commencez à étudier maintenant, belle !", null, null, null },
                    { new Guid("fc290b60-6d8c-4d60-8ac1-48d93121ce56"), new DateTime(2025, 6, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "fr-FR", new Guid("f5a60340-dbb6-4f50-ad84-abaa77905bb2"), "Tu as mangé, jolie ? Si c'est fait, alors... étudie juste une leçon. Les pièces attendent d'être collectées dans ta poche !", null, null, null },
                    { new Guid("fdb01670-3871-401b-a958-1accd1420774"), new DateTime(2025, 6, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "en-US", new Guid("19876859-11c6-4d5d-a043-6325af55817f"), "💰 Techie: \"Today's mission – earn 100 coins!\" Learn a new lesson = new coins. Collect coins daily to redeem great rewards during Self-Study Month!", null, null, null },
                    { new Guid("fe8555aa-3edd-4ceb-a1ce-d9635ec70ec9"), new DateTime(2025, 6, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "fr-FR", new Guid("ba4ccb0d-ee54-46b6-b091-e6c99ee1f224"), "🎁 Choisissez un cours – recevez immédiatement un cadeau de bienvenue ! Chaque cours est une chance de gagner des pièces, de grimper au classement et d’échanger contre un sac mystère FSEL !", null, null, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("044915bf-f8d7-4d10-b0f9-e39d8a4421a5"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("0547e4ca-a9f4-4dcc-97e5-42d2bc4e7978"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("064f51ff-75af-4776-ae1b-4dbeb4bf4021"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("076c6072-0d26-4065-a74f-b774f83c67f2"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("0900d63d-499c-43fc-b159-d69a4b134b22"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("0ce416be-4e34-4efb-89e5-87eb592f48f7"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("0d787987-619a-47c6-a4a8-971e44cd5ae4"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("13787811-66ed-4fdd-8f27-5a059d9021d4"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("28f5127e-d440-46ee-af42-00762465518c"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("2d96bb55-f125-4445-909d-f15b73e4dff1"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("2e97428c-fa50-47c9-ad97-29bd3ea3b246"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("3093e36a-8e21-43c2-92a1-6b62f8e3045e"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("321c1cee-f949-46f0-bbc8-940f2a5881f9"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("33ed40bd-5069-41d2-a9e0-ffd354ae8097"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("346c2afd-3ee8-4b54-8c8e-1bb1399a5ced"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("39a928d0-1610-41e8-8357-3655c1dedc2b"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("3ea49bca-b0eb-4fa0-8b58-79a06e9b1d96"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("43d69e76-fc50-4204-b351-c23157540f28"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("4553653f-3416-43c1-8efc-47657e7ad0ba"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("45734f25-0754-451a-83a8-f2c27ec2dd77"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("494fc24e-d137-4298-805f-6140e990124d"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("4be1d8fd-02f6-46b3-b6c5-96ec75d138af"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("4f6508d1-835b-4378-b5fd-d74ce6b249b9"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("51c2bc5d-641e-4b77-95fa-2199b95c1198"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("59c32b91-ca6f-4312-af18-6dfa9e4c78a1"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("6402dea8-dff7-461e-9e87-e69097a19819"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("6c45046e-2eea-40d8-a207-4e8c8d813a85"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("6d1d4f8b-aa22-4165-8526-de2cf552037a"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("6deaa803-86b7-42aa-b849-20a2ee13760b"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("6f84789f-ee86-4196-8e9a-d6211bd8bf72"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("70991cb8-d0bc-4471-b9c1-4947a93d9f2d"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("77b6a9a9-3c1b-4033-9cd6-9cb60a664230"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("7a2e0d42-7928-4aa7-b997-6271ea70d48b"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("7de5fc5a-b421-4292-b6f7-e740e0551df8"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("82e71cbc-53ed-4dfe-bc05-bd038f14efa0"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("83bb070c-ef7b-43a6-9401-778afe112066"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("8ea67da6-9f4d-4def-8f5b-ff03c6476e98"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("8f847b4f-7843-4b7a-902e-683342d3e0f9"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("97678b67-48ce-40eb-8bc3-b2be51f7809d"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("a2ca820a-c1ce-4bc9-b654-f833e5e9588a"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("aaa34ec6-39da-4345-bb1b-01f71c7b2896"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("aaa5368f-eec1-449f-8db4-89a317e3a234"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("adafdf21-a4fb-4035-bf71-39139bb4fc12"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("b8aba20e-47b3-4911-b5d0-0df5f62da9ed"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("b95dd6ad-c662-4077-bde7-3f723429aacf"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("bdf8a59a-2bea-4000-bf2e-7584ff9153a9"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("bdfb081e-6b75-452b-a927-b9f2add14d06"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("c48bc1b8-9ed3-4aae-ae3b-207820ed4e78"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("d7ade9c0-5444-4584-acdc-e01ebfc6b69c"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("d8fd8423-0d0c-47ab-a20c-025c1c6252d0"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("dd392bfe-9379-43b3-b4b6-e8e0edefa2f5"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("f975284d-589e-4604-a230-e0d2e294557a"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("f985110b-33f6-4282-9217-4d2a93c2a9b4"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("fa3b6ea3-6979-4e9d-b752-f3411fac9214"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("fc290b60-6d8c-4d60-8ac1-48d93121ce56"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("fdb01670-3871-401b-a958-1accd1420774"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("fe8555aa-3edd-4ceb-a1ce-d9635ec70ec9"));

            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("02111ed0-9c34-4c08-ad52-896f843622bb"));

            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("19876859-11c6-4d5d-a043-6325af55817f"));

            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("19c331f9-7aed-4c21-a7a6-dd4b9e90a285"));

            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("2655aa79-8a88-4a1f-9449-1884cb791123"));

            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("56d463aa-463a-4b91-96d9-b50cf16720b9"));

            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("5b08de2c-0de4-4287-a07c-b7def40313e9"));

            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("62413604-f229-4e39-aa96-3b0a3acad3e3"));

            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("63685b29-a8a4-4117-b4ca-dd8e08b9e439"));

            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("65bc348b-99a8-4573-af05-7d4108fc11d2"));

            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("85fe84d6-d116-4b69-86f8-f97849888f87"));

            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("91b551e5-d4f1-4aee-bf08-582678cf5250"));

            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("ba4ccb0d-ee54-46b6-b091-e6c99ee1f224"));

            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("cc34c9bf-e7e1-48db-bd33-c9351aaa7c5f"));

            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("ce9bea9f-dcf9-4a62-8580-218889016932"));

            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("df161229-9390-492b-85b0-b2d0fc61cc89"));

            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("f2f234d2-abab-4bef-8e05-be83776628a8"));

            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("f30517dd-af21-499c-88a2-46a22ad142a3"));

            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("f5a60340-dbb6-4f50-ad84-abaa77905bb2"));

            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("fd440834-703d-4728-a059-bebcd17069d9"));

            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("0b16fc25-ec1d-4677-84a3-6761afa2bb03"),
                column: "Content",
                value: "Day3At7h30");

            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("1c23bddb-426a-4749-95ec-ddcbad71af9e"),
                column: "Content",
                value: "Day1At7h30");

            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("23990ecc-834f-44b7-a7a5-a03baa703b32"),
                column: "Content",
                value: "Day7At19h30");

            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("29fd645f-4e60-4c0a-8266-d24c9854310e"),
                column: "Content",
                value: "OneHourAfterPT");

            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("2d4af96c-5344-42f3-b31f-584fa05adee2"),
                column: "Content",
                value: "Day3At12h00");

            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("3df9fae4-6879-4196-b70e-4776b65fc51a"),
                column: "Content",
                value: "Day2At12h00");

            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("5d324b4a-d608-4e49-9992-b6703c534550"),
                column: "Content",
                value: "Day2At19h30");

            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("8607c5e0-15e4-4a52-9312-ead10428f8c8"),
                column: "Content",
                value: "Day5At12h00");

            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("921cdf08-0201-4a3f-a191-d415c4101018"),
                column: "Content",
                value: "Day4At17h30");

            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("b2de4124-4b28-4120-8156-ddef65cde6bd"),
                column: "Content",
                value: "Day1At19h30");

            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("c15a5dd2-c897-464f-9f1c-5aa8db6cae40"),
                column: "Content",
                value: "Day6At19h30");

            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("c182d087-fe4f-413c-ae1c-0042899e5b16"),
                column: "Content",
                value: "Day7At7h30");

            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("c2a40d5d-6735-40cb-a090-193aa93bcca2"),
                column: "Content",
                value: "Day6At12h00");

            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("e10e8e9e-a132-41f2-b9e8-5a6ec055be2e"),
                column: "Content",
                value: "Day6At17h30");

            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("f398551d-cd3a-4641-bc82-473d718df074"),
                column: "Content",
                value: "Day7At12h00");

            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("fe46415d-4e6e-4248-b828-785f5486b41f"),
                column: "Content",
                value: "Day7At17h30");
        }
    }
}
