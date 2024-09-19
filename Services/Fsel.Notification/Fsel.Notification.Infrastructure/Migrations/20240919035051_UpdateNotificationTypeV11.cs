using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Fsel.Notification.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateNotificationTypeV11 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "NotificationTypeTranslations",
                columns: new[] { "Id", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "IsDeleted", "Language", "NotificationTypeId", "TemplateMessage", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[,]
                {
                    { new Guid("0bbd8db1-48ed-4c8c-ab84-7d22eed9a4b8"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "en-US", new Guid("f9d5a75f-fd55-4b1e-ade8-a446b6465e34"), "Your course will end in the next 2 weeks. Please extend your course subscription to ensure the continuity of your learning journey without interruption", null, null, null },
                    { new Guid("1507e30c-ca03-4370-b186-7bd367c82b7b"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "en-US", new Guid("05441907-46a9-4178-9a69-fce295d670be"), "Your trial learning period will end within the next 2 days. Please extend your course subscription to ensure uninterrupted continuation of your learning journey", null, null, null },
                    { new Guid("381af55c-e89b-43c6-b57c-4c6e240ea0e0"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "fr-FR", new Guid("05441907-46a9-4178-9a69-fce295d670be"), "Votre période d'apprentissage d'essai se terminera dans les 2 prochains jours. Veuillez prolonger votre abonnement au cours pour garantir la poursuite ininterrompue de votre parcours d'apprentissage.", null, null, null },
                    { new Guid("3f03b037-cc3e-47bf-aecc-e1bef7f5bb5d"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "vi-VN", new Guid("05441907-46a9-4178-9a69-fce295d670be"), "Thời gian học thử của bạn sẽ kết thúc trong vòng 2 ngày tới. Bạn vui lòng gia hạn khóa học để đảm bảo tiếp tục hành trình học tập mà không bị gián đoạn nhé", null, null, null },
                    { new Guid("6dee7a96-1062-4543-a3c3-52b0477c2526"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "fr-FR", new Guid("a99bfaae-4139-4766-9405-ef24332fb51a"), "Votre cours se terminera dans les 2 prochains jours. Veuillez prolonger votre abonnement au cours pour assurer la continuité de votre parcours d'apprentissage sans interruption.", null, null, null },
                    { new Guid("782d79bd-8397-4cf9-81f3-f08b44b24958"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "vi-VN", new Guid("a99bfaae-4139-4766-9405-ef24332fb51a"), "Gói học của bạn sẽ kết thúc trong 2 ngày tới.Bạn vui lòng gia hạn khóa học để đảm bảo tiếp tục hành trình học tập mà không bị gián đoạn nhé", null, null, null },
                    { new Guid("a9edcd2d-bab1-42cf-8334-86734573ea3c"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "vi-VN", new Guid("f9d5a75f-fd55-4b1e-ade8-a446b6465e34"), "Gói học của bạn sẽ kết thúc trong 2 tuần tới.Bạn vui lòng gia hạn khóa học để đảm bảo tiếp tục hành trình học tập mà không bị gián đoạn nhé", null, null, null },
                    { new Guid("c5717b67-138b-456c-8b22-d75b7a89cc19"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "en-US", new Guid("a99bfaae-4139-4766-9405-ef24332fb51a"), "Your course will end in the next 2 days. Please extend your course subscription to ensure the continuity of your learning journey without interruption", null, null, null },
                    { new Guid("c803c6ca-2979-4aa8-990c-4a1bf4297f8f"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "fr-FR", new Guid("f9d5a75f-fd55-4b1e-ade8-a446b6465e34"), "Votre cours se terminera dans les 2 prochaines semaines. Veuillez prolonger votre abonnement au cours pour assurer la continuité de votre parcours d'apprentissage sans interruption.", null, null, null }
                });

            migrationBuilder.InsertData(
                table: "NotificationTypes",
                columns: new[] { "Id", "Content", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "Icon", "IsDeleted", "Priority", "TemplateLink", "TemplateMessage", "Type", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[,]
                {
                    { new Guid("0026c94f-7e1f-4cab-ab54-e0bf751f0f92"), "CompletedStreakLogin", new DateTime(2023, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, 1, "/home-chart/daily-checkin", "Bạn đã hoàn thành nhiệm vụ {0} ngày đăng nhập trong tháng. Nhấn để nhận quà cho sự chăm chỉ của mình nào!", "LinkPage", null, null, null },
                    { new Guid("0dd30706-69c5-403b-a488-7b555e47bb60"), "FifthStudyNotice", new DateTime(2023, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, 1, "", "Thông báo quan trọng! Đã 5 ngày kể từ khi bạn truy cập ứng dụng học cuối cùng. Đừng bỏ lỡ cơ hội học tập. Hãy quay lại và tiếp tục hành trình học tập của bạn ngay bây giờ!", "LinkPage", null, null, null },
                    { new Guid("1d44814a-847e-4736-8a95-323148e11f45"), "CompleteCourse", new DateTime(2023, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, 1, "", "Chúc mừng bạn đã hoàn thành khoá học {0}. Nhấn để xem lại hành trình của bạn theo góc nhìn tổng quan nhé.", "LinkPage", null, null, null },
                    { new Guid("2a138a4c-6b3c-4a86-bbc2-3aa845e70c21"), "LuckyTicket", new DateTime(2024, 8, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, 1, "{0}", "Bạn đã nhận được Mã đổi thưởng: {0}. Hãy truy cập leaderboard.fsel.vn để đổi thưởng ngay nào!", "LinkPopup", null, null, null },
                    { new Guid("30f3dce8-e8c0-4231-a2b1-8b97e9551129"), "ExtendSuccessfully", new DateTime(2023, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, 1, "", "Chào mừng bạn đến với FSEL! Hãy chuẩn bị sẵn sàng cho hành trình học tập trong vòng 24 tháng tới nhé!", "LinkPage", null, null, null },
                    { new Guid("3c924341-55de-425d-94e1-c4d090e118a4"), "ThirdStudyNotice", new DateTime(2023, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, 1, "", "Thông báo quan trọng! Đã 3 ngày kể từ lần cuối bạn tham gia vào học tập trên ứng dụng. Hãy dành một ít thời gian mỗi ngày để tiếp tục nâng cao kiến thức của bạn!", "LinkPage", null, null, null },
                    { new Guid("4539a934-7f9a-4f09-8b84-b853092d156d"), "EnergyCollecting", new DateTime(2023, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, 1, "", "Chúng ta chỉ còn một ngày duy nhất để thu thập năng lượng hành tinh và mở ra hành tinh kho báu! Đừng bỏ lỡ cơ hội nhận những phần quà vô cùng hấp dẫn. Hãy bắt đầu ngay bây giờ!", "LinkPage", null, null, null },
                    { new Guid("4a1f848c-f666-48fa-b127-c871af142332"), "UpgradeOrder", new DateTime(2023, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, 1, "/change-level", "Việc học liên tục có thể giúp cải thiện các kỹ năng của bạn nhanh gấp 3 lần so với các bạn học dừng lại. Nhấn để nâng cấp ngay nào!", "LinkPage", null, null, null },
                    { new Guid("4e496aad-c2ce-42a2-8971-71cd28cf3fae"), "FriendPaymentSuccessfully", new DateTime(2023, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, 1, "", "{0} đã đăng kí thành công gói học FSEL,bạn vừa nhận được {1}🟡, nhấn để xem chi tiết ", "LinkPage", null, null, null },
                    { new Guid("4fa09274-1045-4de2-a804-f5fc951a11d0"), "StopBothering", new DateTime(2023, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, 1, "", "Những thông báo này có vẻ không hiệu quả. Chúng tôi sẽ dừng việc gửi thông báo cho bạn...", "LinkPage", null, null, null },
                    { new Guid("5e7e39f8-750a-4709-82b2-12805fc9e2e9"), "FriendCompletedExam", new DateTime(2023, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, 1, "", "{0} đã hoàn thành bài kiểm tra,bạn vừa nhận được 100 xu, nhấn để xem chi tiết", "LinkPage", null, null, null },
                    { new Guid("66349cd9-1e02-432e-8a33-c0b652206d60"), "FirstStudyNotice", new DateTime(2023, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, 1, "", "Chào {0}! Chúng tôi nhận thấy bạn đã không truy cập ứng dụng học trong 1 ngày qua. Hãy quay lại và tiếp tục hành trình học tập của bạn ngay bây giờ!", "LinkPage", null, null, null },
                    { new Guid("69b15507-efe2-49e6-8e03-85e7f18cfdea"), "TreasureExploration", new DateTime(2023, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, 1, "", "Chào {0}! Bạn đã sẵn sàng khám phá hành tinh kho báu Z-Matter của chúng tôi chưa? Thu thập năng lượng hành tinh thông qua nhiệm vụ ngày và tuần để mở ra hành tinh kho báu chứa các phần quà vô cùng hấp dẫn. Hãy khám phá ngay!", "LinkPage", null, null, null },
                    { new Guid("a271bb72-d5cf-449e-a623-cffec444fea8"), "SecondStudyNotice", new DateTime(2023, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, 1, "", "Xin chào! Đã 2 ngày kể từ khi bạn truy cập ứng dụng học lần cuối. Hãy nhớ rằng sự liên tục là chìa khóa cho việc học hiệu quả. Hãy quay lại và tiếp tục nỗ lực nào!", "LinkPage", null, null, null },
                    { new Guid("a3d6129b-9844-448d-aa1c-d6155b89ce6b"), "FriendCompleteUnitOne", new DateTime(2023, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, 1, "", "{0} đã hoàn thành unit 1,bạn vừa nhận được 500 xu, nhấn để xem chi tiết ", "LinkPage", null, null, null },
                    { new Guid("bbbfba53-8560-4ded-94dc-8dc3b505286c"), "FourthStudyNotice", new DateTime(2023, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, 1, "", "Cảnh báo! Bạn đã không học trong 4 ngày qua. Hãy dành ít nhất một chút thời gian mỗi ngày để duy trì và phát triển kiến thức của mình", "LinkPage", null, null, null },
                    { new Guid("caca0a25-2196-4536-a778-8e08d605691f"), "SubcriptionNotice", new DateTime(2023, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, 1, "/account?setting=mySubscription", "Các nội dung học thuật có thể bị khoá lại sau 7 ngày nữa. Cơ hội cuối, gia hạn gói ngay nào!", "LinkPage", null, null, null }
                });

            migrationBuilder.InsertData(
                table: "NotificationTypeTranslations",
                columns: new[] { "Id", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "IsDeleted", "Language", "NotificationTypeId", "TemplateMessage", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[,]
                {
                    { new Guid("0023c610-278d-4afc-88d0-bd546a7eaba4"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "en-US", new Guid("4fa09274-1045-4de2-a804-f5fc951a11d0"), "These notifications don't seem to be effective. We'll stop sending them for now...", null, null, null },
                    { new Guid("0484f322-e5ad-45f0-a467-eb93b47f4aa1"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "fr-FR", new Guid("4fa09274-1045-4de2-a804-f5fc951a11d0"), "Ces notifications ne semblent pas être efficaces. Nous allons arrêter de les envoyer pour le moment...", null, null, null },
                    { new Guid("048b78e3-4ff8-4919-b5b0-990c00bf4aaa"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "vi-VN", new Guid("bbbfba53-8560-4ded-94dc-8dc3b505286c"), "Cảnh báo! Bạn đã không học trong 4 ngày qua. Hãy dành ít nhất một chút thời gian mỗi ngày để duy trì và phát triển kiến thức của mình", null, null, null },
                    { new Guid("04f45f50-c7bf-4062-a139-23ac1983a902"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "en-US", new Guid("1d44814a-847e-4736-8a95-323148e11f45"), "Congratulations on completing the {0} course! Click to see an overview of your journey.", null, null, null },
                    { new Guid("06beee4e-1723-4079-8cf4-ede3f755b2f7"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "fr-FR", new Guid("a3d6129b-9844-448d-aa1c-d6155b89ce6b"), "{0} a terminé l'Unité 1, et vous venez de recevoir 500 xu. Appuyez pour voir les détails.", null, null, null },
                    { new Guid("0bf56c51-7b72-4c5e-b9cd-806ee3e9fc8e"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "fr-FR", new Guid("66349cd9-1e02-432e-8a33-c0b652206d60"), "Bonjour {0} ! Nous avons remarqué que vous n'avez pas accédé à FSEL depuis un jour. Revenez et continuez votre parcours d'apprentissage maintenant !", null, null, null },
                    { new Guid("14f46183-a3e8-4662-8c68-58e2c3a8f75f"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "fr-FR", new Guid("3c924341-55de-425d-94e1-c4d090e118a4"), "Avis important ! Cela fait 3 jours que vous n'avez pas interagi avec FSEL. Prenez un peu de temps chaque jour pour continuer à élargir vos connaissances !", null, null, null },
                    { new Guid("15cc8ece-544e-4247-95f7-1913b7588a7e"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "en-US", new Guid("69b15507-efe2-49e6-8e03-85e7f18cfdea"), "Hello {0}! Are you ready to explore our treasure planet Z-Matter? Collect planetary energy through daily and weekly missions to unlock the treasure planet with full of exciting rewards. Start exploring now!", null, null, null },
                    { new Guid("22e0b002-3122-4d83-b238-7496153cba03"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "vi-VN", new Guid("a3d6129b-9844-448d-aa1c-d6155b89ce6b"), "{0} đã hoàn thành unit 1,bạn vừa nhận được 500 xu, nhấn để xem chi tiết ", null, null, null },
                    { new Guid("27a848b9-c19d-4498-bea8-100c1044bbc4"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "en-US", new Guid("bbbfba53-8560-4ded-94dc-8dc3b505286c"), "Warning! You haven't studied in the past 4 days. Make sure to spend at least a little time each day to maintain and grow your knowledge.", null, null, null },
                    { new Guid("29543af0-ea24-4e6d-a585-40640b501c7c"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "fr-FR", new Guid("4539a934-7f9a-4f09-8b84-b853092d156d"), "Il ne nous reste qu'un jour pour collecter l'énergie planétaire et déverrouiller la planète aux trésors ! Ne manquez pas les incroyables récompenses. Commençons maintenant !", null, null, null },
                    { new Guid("3172e81e-b527-488c-b87c-c75dfb6ce299"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "en-US", new Guid("4a1f848c-f666-48fa-b127-c871af142332"), "Continuous learning can help improve your skills 3 times faster than your peers who stop studying. Click to upgrade now!", null, null, null },
                    { new Guid("31a6c1b5-78ed-4ae9-9911-eddaf1adf3a8"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "fr-FR", new Guid("0dd30706-69c5-403b-a488-7b555e47bb60"), "Avis important ! Cela fait 5 jours que vous n'avez pas accédé à FSEL. Ne manquez pas les opportunités d'apprentissage. Revenez et poursuivez votre parcours maintenant !", null, null, null },
                    { new Guid("3cda5e20-b48e-49ff-945e-6a07d4ebe495"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "vi-VN", new Guid("0dd30706-69c5-403b-a488-7b555e47bb60"), "Thông báo quan trọng! Đã 5 ngày kể từ khi bạn truy cập ứng dụng học cuối cùng. Đừng bỏ lỡ cơ hội học tập. Hãy quay lại và tiếp tục hành trình học tập của bạn ngay bây giờ!", null, null, null },
                    { new Guid("41150d47-6270-4420-8a08-627bf4babd45"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "en-US", new Guid("5e7e39f8-750a-4709-82b2-12805fc9e2e9"), "{0} has completed the placement test, and you have just received 100 xu. Tap to see details.", null, null, null },
                    { new Guid("4acdfde0-273f-40cc-8f21-b05ab07921b4"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "en-US", new Guid("a3d6129b-9844-448d-aa1c-d6155b89ce6b"), "{0} has completed Unit 1, and you have just received 500 xu. Tap to see details", null, null, null },
                    { new Guid("4d4d5989-d6c3-459a-b4df-1a9708f5b1dd"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "en-US", new Guid("caca0a25-2196-4536-a778-8e08d605691f"), "Your subscription will expire and the course content will be locked in 7 days. Please renew your subscription to continue learning!", null, null, null },
                    { new Guid("5046975d-c76d-4f46-8ede-fa4b766c6ce5"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "vi-VN", new Guid("4e496aad-c2ce-42a2-8971-71cd28cf3fae"), "{0} đã đăng kí thành công gói học FSEL,bạn vừa nhận được {1} xu, nhấn để xem chi tiết ", null, null, null },
                    { new Guid("546d12f5-4634-44a6-9bc1-8b195a29cea0"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "en-US", new Guid("4e496aad-c2ce-42a2-8971-71cd28cf3fae"), "{0} has successfully enrolled in the FSEL course package, and you have just received {1} xu. Tap to see details.", null, null, null },
                    { new Guid("593ca78c-074b-4da6-bc31-421a672c04ec"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "fr-FR", new Guid("5e7e39f8-750a-4709-82b2-12805fc9e2e9"), "{0} a terminé le test de positionnement, et vous venez de recevoir 100 xu. Appuyez pour voir les détails.", null, null, null },
                    { new Guid("5bee37bd-d815-4433-8bde-e54e7c77ca25"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "vi-VN", new Guid("3c924341-55de-425d-94e1-c4d090e118a4"), "Thông báo quan trọng! Đã 3 ngày kể từ lần cuối bạn tham gia vào học tập trên ứng dụng. Hãy dành một ít thời gian mỗi ngày để tiếp tục nâng cao kiến thức của bạn!", null, null, null },
                    { new Guid("67aa0a4f-f3a8-4575-ac31-4874146286da"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "vi-VN", new Guid("a271bb72-d5cf-449e-a623-cffec444fea8"), "Xin chào! Đã 2 ngày kể từ khi bạn truy cập ứng dụng học lần cuối. Hãy nhớ rằng sự liên tục là chìa khóa cho việc học hiệu quả. Hãy quay lại và tiếp tục nỗ lực nào!", null, null, null },
                    { new Guid("68e7c72b-c487-4e14-8272-518dcaae1998"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "vi-VN", new Guid("0026c94f-7e1f-4cab-ab54-e0bf751f0f92"), "Bạn đã hoàn thành nhiệm vụ {0} ngày đăng nhập trong tháng. Nhấn để nhận quà cho sự chăm chỉ của mình nào!", null, null, null },
                    { new Guid("74cee264-6121-4403-8f1e-d770db17e533"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "fr-FR", new Guid("caca0a25-2196-4536-a778-8e08d605691f"), "Votre abonnement expirera et le contenu du cours sera verrouillé dans 7 jours. Veuillez renouveler votre abonnement pour continuer à apprendre !", null, null, null },
                    { new Guid("7c42c432-d9b7-47e4-85f4-23b6acaedef2"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "en-US", new Guid("66349cd9-1e02-432e-8a33-c0b652206d60"), "Hello {0}! We noticed you haven't accessed FSEL in the past day. Come back and continue your learning journey now!", null, null, null },
                    { new Guid("7c5b76ab-7a31-4d32-b332-b4c74d43bdbc"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "vi-VN", new Guid("66349cd9-1e02-432e-8a33-c0b652206d60"), "Chào {0}! Chúng tôi nhận thấy bạn đã không truy cập ứng dụng học trong 1 ngày qua. Hãy quay lại và tiếp tục hành trình học tập của bạn ngay bây giờ!", null, null, null },
                    { new Guid("80cd4472-84e9-4888-9340-6cb37745db5b"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "en-US", new Guid("a271bb72-d5cf-449e-a623-cffec444fea8"), "Hello! It's been 2 days since you last accessed FSEL. Remember, consistency is key to effective learning. Let's get back and keep the momentum going!", null, null, null },
                    { new Guid("81fbb72a-ab26-44b4-824a-ff28b7edc276"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "vi-VN", new Guid("4a1f848c-f666-48fa-b127-c871af142332"), "Chúc mừng bạn đã hoàn thành khoá học {0}. Nhấn để xem lại hành trình của bạn theo góc nhìn tổng quan nhé.", null, null, null },
                    { new Guid("873b1b28-f893-4c34-9158-e4581475d018"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "fr-FR", new Guid("0026c94f-7e1f-4cab-ab54-e0bf751f0f92"), "Vous avez accompli la tâche de {0} jours de connexion dans le mois. Cliquez pour recevoir un cadeau pour votre dévouement !", null, null, null },
                    { new Guid("aa0bc23b-2281-4e19-bdff-10c87dfb6152"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "en-US", new Guid("30f3dce8-e8c0-4231-a2b1-8b97e9551129"), "Welcome to FSEL! Get ready for your learning journey over the next 3 months!", null, null, null },
                    { new Guid("ab10dee3-6d3b-4482-92ea-65dcac210fe0"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "vi-VN", new Guid("5e7e39f8-750a-4709-82b2-12805fc9e2e9"), "{0} đã hoàn thành bài kiểm tra,bạn vừa nhận được 100 xu, nhấn để xem chi tiết", null, null, null },
                    { new Guid("ac59309d-af2b-43d7-adcc-c8bdfd8cdde5"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "vi-VN", new Guid("4539a934-7f9a-4f09-8b84-b853092d156d"), "Chúng ta chỉ còn một ngày duy nhất để thu thập năng lượng hành tinh và mở ra hành tinh kho báu! Đừng bỏ lỡ cơ hội nhận những phần quà vô cùng hấp dẫn. Hãy bắt đầu ngay bây giờ!", null, null, null },
                    { new Guid("ad1fecc2-2420-4d89-80e5-240dc9de051c"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "en-US", new Guid("3c924341-55de-425d-94e1-c4d090e118a4"), "Important Notice! It's been 3 days since you last engaged with FSEL. Take a little time each day to keep expanding your knowledge!", null, null, null },
                    { new Guid("af3164db-0b89-4a63-ab21-4956600ec1d8"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "fr-FR", new Guid("a271bb72-d5cf-449e-a623-cffec444fea8"), "Bonjour ! Cela fait 2 jours que vous n'avez pas accédé à FSEL. N'oubliez pas, la constance est la clé d'un apprentissage efficace. Revenons et gardons l'élan !", null, null, null },
                    { new Guid("b05580f6-5831-4b87-9edb-05faaec04cd4"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "vi-VN", new Guid("4fa09274-1045-4de2-a804-f5fc951a11d0"), "Những thông báo này có vẻ không hiệu quả. Chúng tôi sẽ dừng việc gửi thông báo cho bạn...", null, null, null },
                    { new Guid("b7b00477-9cbf-4ce6-8254-e8374ab4240a"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "fr-FR", new Guid("69b15507-efe2-49e6-8e03-85e7f18cfdea"), "Bonjour {0} ! Êtes-vous prêt à explorer notre planète aux trésors Z-Matter ? Collectez de l'énergie planétaire grâce aux missions quotidiennes et hebdomadaires pour déverrouiller la planète aux trésors remplie de récompenses excitantes. Commencez l'exploration dès maintenant !", null, null, null },
                    { new Guid("bf164374-05fa-43ce-9bbc-982fc571beab"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "en-US", new Guid("0dd30706-69c5-403b-a488-7b555e47bb60"), "Important Notice! It's been 5 days since you last accessed FSEL. Don't miss out on learning opportunities. Come back and continue your journey now!", null, null, null },
                    { new Guid("c1c64905-b7e5-454a-ac72-982588327fe2"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "fr-FR", new Guid("1d44814a-847e-4736-8a95-323148e11f45"), "Félicitations pour avoir terminé le cours {0} ! Cliquez pour voir un aperçu de votre parcours.", null, null, null },
                    { new Guid("cfe34390-800d-44b7-8e11-17f5aa2daa76"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "en-US", new Guid("4539a934-7f9a-4f09-8b84-b853092d156d"), "We have only one day left to collect planetary energy and unlock the treasure planet! Don't miss out on the amazing rewards. Let's get started now!", null, null, null },
                    { new Guid("d2921fd1-4999-4f4f-9472-f9541e55de14"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "vi-VN", new Guid("1d44814a-847e-4736-8a95-323148e11f45"), "Chúc mừng bạn đã hoàn thành khoá học {0}. Nhấn để xem lại hành trình của bạn theo góc nhìn tổng quan nhé.", null, null, null },
                    { new Guid("e01358a7-eae7-4e2e-b37b-2d73e83ca17a"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "fr-FR", new Guid("bbbfba53-8560-4ded-94dc-8dc3b505286c"), "Avertissement ! Vous n'avez pas étudié au cours des 4 derniers jours. Assurez-vous de passer au moins un peu de temps chaque jour pour maintenir et développer vos connaissances.", null, null, null },
                    { new Guid("e4b0984e-ea8a-4c33-aba5-4dbe7ac5f630"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "fr-FR", new Guid("4a1f848c-f666-48fa-b127-c871af142332"), "L'apprentissage continu peut aider à améliorer vos compétences 3 fois plus rapidement que vos pairs qui arrêtent d'étudier. Cliquez pour mettre à niveau maintenant !", null, null, null },
                    { new Guid("ebbd7083-4e2e-4f1f-bb6a-faebb371a3ce"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "fr-FR", new Guid("30f3dce8-e8c0-4231-a2b1-8b97e9551129"), "Bienvenue chez FSEL ! Préparez-vous pour votre parcours d'apprentissage au cours des 3 prochains mois !", null, null, null },
                    { new Guid("edc69722-e530-4db7-893c-8058226e7236"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "vi-VN", new Guid("caca0a25-2196-4536-a778-8e08d605691f"), "Các nội dung học thuật có thể bị khoá lại sau 7 ngày nữa. Cơ hội cuối, gia hạn gói ngay nào!", null, null, null },
                    { new Guid("efe2e116-ac19-46f0-9133-607694e6b926"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "vi-VN", new Guid("69b15507-efe2-49e6-8e03-85e7f18cfdea"), "Chào {0}! Bạn đã sẵn sàng khám phá hành tinh kho báu Z-Matter của chúng tôi chưa? Thu thập năng lượng hành tinh thông qua nhiệm vụ ngày và tuần để mở ra hành tinh kho báu chứa các phần quà vô cùng hấp dẫn. Hãy khám phá ngay!", null, null, null },
                    { new Guid("f674d530-9fce-4990-95ff-82164318bf8c"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "fr-FR", new Guid("4e496aad-c2ce-42a2-8971-71cd28cf3fae"), "{0} s'est inscrit avec succès au forfait de cours FSEL, et vous venez de recevoir {1} xu. Appuyez pour voir les détails.", null, null, null },
                    { new Guid("f7752d4f-8b49-4f8a-b860-b1d1ceea6b49"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "en-US", new Guid("0026c94f-7e1f-4cab-ab54-e0bf751f0f92"), "You have completed the task {0} login days within the month. Click to receive a gift for your dedication!", null, null, null },
                    { new Guid("fcf43e61-7a49-4978-b547-85de6aea5363"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "vi-VN", new Guid("30f3dce8-e8c0-4231-a2b1-8b97e9551129"), "Chào mừng bạn đến với FSEL! Hãy chuẩn bị sẵn sàng cho hành trình học tập trong vòng 24 tháng tới nhé!", null, null, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("0023c610-278d-4afc-88d0-bd546a7eaba4"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("0484f322-e5ad-45f0-a467-eb93b47f4aa1"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("048b78e3-4ff8-4919-b5b0-990c00bf4aaa"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("04f45f50-c7bf-4062-a139-23ac1983a902"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("06beee4e-1723-4079-8cf4-ede3f755b2f7"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("0bbd8db1-48ed-4c8c-ab84-7d22eed9a4b8"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("0bf56c51-7b72-4c5e-b9cd-806ee3e9fc8e"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("14f46183-a3e8-4662-8c68-58e2c3a8f75f"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("1507e30c-ca03-4370-b186-7bd367c82b7b"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("15cc8ece-544e-4247-95f7-1913b7588a7e"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("22e0b002-3122-4d83-b238-7496153cba03"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("27a848b9-c19d-4498-bea8-100c1044bbc4"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("29543af0-ea24-4e6d-a585-40640b501c7c"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("3172e81e-b527-488c-b87c-c75dfb6ce299"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("31a6c1b5-78ed-4ae9-9911-eddaf1adf3a8"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("381af55c-e89b-43c6-b57c-4c6e240ea0e0"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("3cda5e20-b48e-49ff-945e-6a07d4ebe495"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("3f03b037-cc3e-47bf-aecc-e1bef7f5bb5d"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("41150d47-6270-4420-8a08-627bf4babd45"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("4acdfde0-273f-40cc-8f21-b05ab07921b4"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("4d4d5989-d6c3-459a-b4df-1a9708f5b1dd"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("5046975d-c76d-4f46-8ede-fa4b766c6ce5"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("546d12f5-4634-44a6-9bc1-8b195a29cea0"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("593ca78c-074b-4da6-bc31-421a672c04ec"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("5bee37bd-d815-4433-8bde-e54e7c77ca25"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("67aa0a4f-f3a8-4575-ac31-4874146286da"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("68e7c72b-c487-4e14-8272-518dcaae1998"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("6dee7a96-1062-4543-a3c3-52b0477c2526"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("74cee264-6121-4403-8f1e-d770db17e533"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("782d79bd-8397-4cf9-81f3-f08b44b24958"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("7c42c432-d9b7-47e4-85f4-23b6acaedef2"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("7c5b76ab-7a31-4d32-b332-b4c74d43bdbc"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("80cd4472-84e9-4888-9340-6cb37745db5b"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("81fbb72a-ab26-44b4-824a-ff28b7edc276"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("873b1b28-f893-4c34-9158-e4581475d018"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("a9edcd2d-bab1-42cf-8334-86734573ea3c"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("aa0bc23b-2281-4e19-bdff-10c87dfb6152"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("ab10dee3-6d3b-4482-92ea-65dcac210fe0"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("ac59309d-af2b-43d7-adcc-c8bdfd8cdde5"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("ad1fecc2-2420-4d89-80e5-240dc9de051c"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("af3164db-0b89-4a63-ab21-4956600ec1d8"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("b05580f6-5831-4b87-9edb-05faaec04cd4"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("b7b00477-9cbf-4ce6-8254-e8374ab4240a"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("bf164374-05fa-43ce-9bbc-982fc571beab"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("c1c64905-b7e5-454a-ac72-982588327fe2"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("c5717b67-138b-456c-8b22-d75b7a89cc19"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("c803c6ca-2979-4aa8-990c-4a1bf4297f8f"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("cfe34390-800d-44b7-8e11-17f5aa2daa76"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("d2921fd1-4999-4f4f-9472-f9541e55de14"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("e01358a7-eae7-4e2e-b37b-2d73e83ca17a"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("e4b0984e-ea8a-4c33-aba5-4dbe7ac5f630"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("ebbd7083-4e2e-4f1f-bb6a-faebb371a3ce"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("edc69722-e530-4db7-893c-8058226e7236"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("efe2e116-ac19-46f0-9133-607694e6b926"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("f674d530-9fce-4990-95ff-82164318bf8c"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("f7752d4f-8b49-4f8a-b860-b1d1ceea6b49"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("fcf43e61-7a49-4978-b547-85de6aea5363"));

            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("2a138a4c-6b3c-4a86-bbc2-3aa845e70c21"));

            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("0026c94f-7e1f-4cab-ab54-e0bf751f0f92"));

            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("0dd30706-69c5-403b-a488-7b555e47bb60"));

            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("1d44814a-847e-4736-8a95-323148e11f45"));

            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("30f3dce8-e8c0-4231-a2b1-8b97e9551129"));

            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("3c924341-55de-425d-94e1-c4d090e118a4"));

            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("4539a934-7f9a-4f09-8b84-b853092d156d"));

            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("4a1f848c-f666-48fa-b127-c871af142332"));

            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("4e496aad-c2ce-42a2-8971-71cd28cf3fae"));

            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("4fa09274-1045-4de2-a804-f5fc951a11d0"));

            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("5e7e39f8-750a-4709-82b2-12805fc9e2e9"));

            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("66349cd9-1e02-432e-8a33-c0b652206d60"));

            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("69b15507-efe2-49e6-8e03-85e7f18cfdea"));

            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("a271bb72-d5cf-449e-a623-cffec444fea8"));

            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("a3d6129b-9844-448d-aa1c-d6155b89ce6b"));

            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("bbbfba53-8560-4ded-94dc-8dc3b505286c"));

            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("caca0a25-2196-4536-a778-8e08d605691f"));
        }
    }
}
