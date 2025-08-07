using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Fsel.Notification.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_NotificationTypeJson_Add_NotifyAfterPT : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "NotificationTypes",
                columns: new[] { "Id", "Content", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "Icon", "IsDeleted", "Priority", "TemplateLink", "TemplateMessage", "Type", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[,]
                {
                    { new Guid("0b16fc25-ec1d-4677-84a3-6761afa2bb03"), "Day3At7h30", new DateTime(2025, 5, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, 1, "/learn", "🌌 Techie: “Hôm nay bạn muốn khám phá hành tinh nào?” Mỗi bài học là một hành tinh mới. Vào chọn bài và khởi động tàu nhé!", "Text", null, null, null },
                    { new Guid("1c23bddb-426a-4749-95ec-ddcbad71af9e"), "Day1At7h30", new DateTime(2025, 5, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, 1, "/learn", "👨‍🚀 Trung tâm chỉ huy đang chờ bạn phản hồi! 🤖 Techie đã định vị lộ trình. Hãy vào học bài đầu để khởi động chuyến bay!", "Text", null, null, null },
                    { new Guid("23990ecc-834f-44b7-a7a5-a03baa703b32"), "Day7At19h30", new DateTime(2025, 5, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, 1, "/learn", "📊 AI dự đoán bạn sẽ học bài hôm nay …Nếu đúng, Techie sẽ thưởng bạn 160 coin. Nếu sai… Techie sẽ buồn thêm lần nữa 😞", "Text", null, null, null },
                    { new Guid("29fd645f-4e60-4c0a-8266-d24c9854310e"), "OneHourAfterPT", new DateTime(2025, 5, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, 1, "/marketplace", "🚨 Cảnh báo từ trung tâm FSEL Store: Có người vừa đổi quà trước bạn 1 bước! Coin bạn vẫn còn... nhưng quà thì không chắc 😬", "Text", null, null, null },
                    { new Guid("2d4af96c-5344-42f3-b31f-584fa05adee2"), "Day3At12h00", new DateTime(2025, 5, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, 1, "/marketplace", "⚠️ Đội bạn đã đến sao Thưởng Quà! Techie nhắc: Hãy vào học để kịp lấy coin và ghé FSEL Store trước khi kho báu đóng lại!", "Text", null, null, null },
                    { new Guid("3df9fae4-6879-4196-b70e-4776b65fc51a"), "Day2At12h00", new DateTime(2025, 5, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, 1, "/learn", "🤪 Techie tẻn tẻn khắp vũ trụ đi tìm bạn học tiếp đây nè! Không thấy bạn đâu hết trơn, vào học lẹ đi để tớ còn phát xu!", "Text", null, null, null },
                    { new Guid("5d324b4a-d608-4e49-9992-b6703c534550"), "Day2At19h30", new DateTime(2025, 5, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, 1, "/learn", "🌼 Cô nàng, anh chàng thư giãn giờ này chắc đang nằm lướt TikTok? Lướt xong thì học nhẹ 1 bài cùng Techie rồi chill tiếp nhé!", "Text", null, null, null },
                    { new Guid("8607c5e0-15e4-4a52-9312-ead10428f8c8"), "Day5At12h00", new DateTime(2025, 5, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, 1, "/learn", "✨ 8386 là phát tài phát lộc, còn học là phát xu phát quà! Mỗi bài học hôm nay sẽ giúp bạn tiến gần hơn tới phần thưởng khủng từ FSEL Store!", "Text", null, null, null },
                    { new Guid("921cdf08-0201-4a3f-a191-d415c4101018"), "Day4At17h30", new DateTime(2025, 5, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, 1, "/learn", "👀 Techie đang check VAR: bạn hôm nay chưa học bài nào luôn á! VAR xác nhận: coin đang bị bỏ lỡ, cơ hội bóc Túi Mù đang chờ bạn đó!", "Text", null, null, null },
                    { new Guid("b2de4124-4b28-4120-8156-ddef65cde6bd"), "Day1At19h30", new DateTime(2025, 5, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, 1, "/learn", "🤖 Techie: \"Chắc bạn bị bắt cóc khỏi vũ trụ học mất rồi?\" Nếu bạn còn nhớ mật khẩu… vào học bài đầu tiên để nhận lại quyền truy cập vũ trụ nhé!", "Text", null, null, null },
                    { new Guid("c15a5dd2-c897-464f-9f1c-5aa8db6cae40"), "Day6At19h30", new DateTime(2025, 5, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, 1, "/learn", "Cơm thì ăn đều, mà bài thì bỏ đó? 😅 Techie thấy bạn có tiềm năng đổi iPhone nếu học đều đó. Vào học lẹ đi người đẹp!", "Text", null, null, null },
                    { new Guid("c182d087-fe4f-413c-ae1c-0042899e5b16"), "Day7At7h30", new DateTime(2025, 5, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, 1, "/learn", "🌞 Dậy chưa đó? Techie nhắc nhẹ nè! Nhớ dành chút thời gian hôm nay để vào học cùng Techie nha – xu vẫn đang rơi đều, quà vẫn đang chờ bạn gom!", "Text", null, null, null },
                    { new Guid("c2a40d5d-6735-40cb-a090-193aa93bcca2"), "Day6At12h00", new DateTime(2025, 5, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, 1, "/learn", "😅 Cả nhóm học tới bài 4 rồi đó, còn bạn thì… Techie giữ chỗ cho bạn học cùng nè. Vào lẹ kẻo tụt mood học nhóm!", "Text", null, null, null },
                    { new Guid("e10e8e9e-a132-41f2-b9e8-5a6ec055be2e"), "Day6At17h30", new DateTime(2025, 5, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, 1, "/learn", "⏳ Đừng để hôm nay trôi qua mà chưa chọn khóa học! Học sớm hơn = nhận coin sớm hơn! Bắt đầu từ bài đầu tiên ngay nhé!", "Text", null, null, null },
                    { new Guid("f398551d-cd3a-4641-bc82-473d718df074"), "Day7At12h00", new DateTime(2025, 5, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, 1, "/marketplace", "👀 Techie đang check VAR: bạn hôm nay chưa học bài nào luôn á! VAR xác nhận: coin đang bị bỏ lỡ, cơ hội bóc Túi Mù đang chờ bạn đó!", "Text", null, null, null },
                    { new Guid("fe46415d-4e6e-4248-b828-785f5486b41f"), "Day7At17h30", new DateTime(2025, 5, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, 1, "/learn", "📊 AI dự đoán bạn sẽ học bài hôm nay …Nếu đúng, Techie sẽ thưởng bạn 160 coin. Nếu sai… Techie sẽ buồn thêm lần nữa 😞", "Text", null, null, null }
                });

            migrationBuilder.InsertData(
                table: "NotificationTypeTranslations",
                columns: new[] { "Id", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "IsDeleted", "Language", "NotificationTypeId", "TemplateMessage", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[,]
                {
                    { new Guid("0042ee0c-6d9c-445c-905d-e5e76ffb6662"), new DateTime(2025, 5, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "vi-VN", new Guid("fe46415d-4e6e-4248-b828-785f5486b41f"), "📊 AI dự đoán bạn sẽ học bài hôm nay …Nếu đúng, Techie sẽ thưởng bạn 160 coin. Nếu sai… Techie sẽ buồn thêm lần nữa 😞", null, null, null },
                    { new Guid("029d8556-2d49-473f-8085-dc60568801d9"), new DateTime(2025, 5, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "fr-FR", new Guid("f398551d-cd3a-4641-bc82-473d718df074"), "👀 Techie vérifie VAR : tu n'as pas étudié de leçon aujourd'hui ! VAR confirme : des pièces sont manquées, et l'opportunité du Sac Mystère t'attend !", null, null, null },
                    { new Guid("05880e81-f2b7-4a75-86ac-97191bde46d5"), new DateTime(2025, 5, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "en-US", new Guid("fe46415d-4e6e-4248-b828-785f5486b41f"), "📊 AI predicts you'll study today… If correct, Techie will reward you with 160 coins. If wrong… Techie will be sad again 😞", null, null, null },
                    { new Guid("07f77fa5-27f7-4b55-9d1a-505e3876daa4"), new DateTime(2025, 5, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "fr-FR", new Guid("23990ecc-834f-44b7-a7a5-a03baa703b32"), "📊 L'IA prévoit que tu vas étudier aujourd'hui… Si c'est vrai, Techie te récompensera avec 160 pièces. Si c'est faux... Techie sera encore triste 😞", null, null, null },
                    { new Guid("0823874f-fc87-428e-8e3d-91620693586a"), new DateTime(2025, 5, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "en-US", new Guid("8607c5e0-15e4-4a52-9312-ead10428f8c8"), "✨ 8386 is prosperity and wealth, and studying leads to coins and rewards! Every lesson today will bring you closer to the big prizes from the FSEL Store!", null, null, null },
                    { new Guid("0a75a7f0-aae1-4091-a773-9ec146634a3e"), new DateTime(2025, 5, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "en-US", new Guid("b2de4124-4b28-4120-8156-ddef65cde6bd"), "🤖 Techie: \"Did you get kidnapped from the learning universe?\" If you still remember the password... come back and take the first lesson to regain access to the universe!", null, null, null },
                    { new Guid("14641324-e185-4bab-be4c-9aaad65d9b5f"), new DateTime(2025, 5, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "fr-FR", new Guid("29fd645f-4e60-4c0a-8266-d24c9854310e"), "🚨 Alerte du centre FSEL Store: Quelqu'un vient de récupérer son cadeau avant vous ! Vos pièces sont encore là... mais le cadeau pourrait être parti 😬", null, null, null },
                    { new Guid("15f70c1e-72ad-4c88-8b5b-0f8fa7cbbdb1"), new DateTime(2025, 5, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "en-US", new Guid("29fd645f-4e60-4c0a-8266-d24c9854310e"), "🚨 Warning from FSEL Store: Someone just redeemed their reward before you! Your coins are still there... but the reward might be gone 😬", null, null, null },
                    { new Guid("172a6057-f27b-4194-9fba-a49801ba9f95"), new DateTime(2025, 5, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "fr-FR", new Guid("1c23bddb-426a-4749-95ec-ddcbad71af9e"), "👨‍🚀 Le centre de commande attend votre réponse ! 🤖 Techie a défini le parcours d'apprentissage. Commencez votre voyage en prenant la première leçon !", null, null, null },
                    { new Guid("1d6b4320-f4f7-468e-bd27-8ac2c3309fc8"), new DateTime(2025, 5, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "en-US", new Guid("5d324b4a-d608-4e49-9992-b6703c534550"), "🌼 Are you the chill type, probably scrolling TikTok right now? After scrolling, just study one lesson with Techie and then chill some more!", null, null, null },
                    { new Guid("1e0f89d5-fa98-4ebb-90df-a1fe34d851a8"), new DateTime(2025, 5, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "vi-VN", new Guid("921cdf08-0201-4a3f-a191-d415c4101018"), "👀 Techie đang check VAR: bạn hôm nay chưa học bài nào luôn á! VAR xác nhận: coin đang bị bỏ lỡ, cơ hội bóc Túi Mù đang chờ bạn đó!", null, null, null },
                    { new Guid("1ef2fb87-d639-4514-a5d3-8fa14ee5f7d6"), new DateTime(2025, 5, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "fr-FR", new Guid("b2de4124-4b28-4120-8156-ddef65cde6bd"), "🤖 Techie : \"Tu as été kidnappé de l'univers d'apprentissage ?\" Si tu te souviens du mot de passe... viens apprendre la première leçon pour retrouver l'accès à l'univers !", null, null, null },
                    { new Guid("1ef93023-c60c-4803-93f0-2b166c49cf5b"), new DateTime(2025, 5, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "en-US", new Guid("0b16fc25-ec1d-4677-84a3-6761afa2bb03"), "🌌 Techie: \"Which planet do you want to explore today?\" Each lesson is a new planet. Choose a lesson and start your journey!", null, null, null },
                    { new Guid("2171fb8f-d8ae-404b-b6a6-6191841ed706"), new DateTime(2025, 5, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "en-US", new Guid("3df9fae4-6879-4196-b70e-4776b65fc51a"), "🤪 Techie is searching the universe to find you and continue the lesson! Can’t find you anywhere, hurry up and start studying so I can give you some coins!", null, null, null },
                    { new Guid("244e2fc4-0bf1-4a69-a28a-0b9cd5a5ed86"), new DateTime(2025, 5, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "vi-VN", new Guid("5d324b4a-d608-4e49-9992-b6703c534550"), "🌼 Cô nàng, anh chàng thư giãn giờ này chắc đang nằm lướt TikTok? Lướt xong thì học nhẹ 1 bài cùng Techie rồi chill tiếp nhé!", null, null, null },
                    { new Guid("2a0092c6-47f1-43b4-b7af-b08cad56173f"), new DateTime(2025, 5, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "en-US", new Guid("c182d087-fe4f-413c-ae1c-0042899e5b16"), "🌞 Are you awake yet? Techie gently reminds you! Remember to spend a little time today to study with Techie – coins are still falling, and rewards are waiting for you to collect!", null, null, null },
                    { new Guid("3a9c93c7-35a3-49a5-93ed-2fff62a91c9e"), new DateTime(2025, 5, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "fr-FR", new Guid("2d4af96c-5344-42f3-b31f-584fa05adee2"), "⚠️ Votre équipe est arrivée sur la planète des prix ! Techie vous rappelle : Rejoignez la leçon pour obtenir des pièces et visiter le FSEL Store avant que le trésor ne se ferme !", null, null, null },
                    { new Guid("3d1b042a-4f7a-4d38-bcdc-af4985d62a89"), new DateTime(2025, 5, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "fr-FR", new Guid("c182d087-fe4f-413c-ae1c-0042899e5b16"), "🌞 Tu es réveillé ? Techie te rappelle gentiment ! N'oublie pas de passer un peu de temps aujourd'hui pour étudier avec Techie – les pièces tombent encore et les récompenses t'attendent pour être collectées !", null, null, null },
                    { new Guid("6083c4b5-9d3a-4720-9f86-b41eec30e098"), new DateTime(2025, 5, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "vi-VN", new Guid("1c23bddb-426a-4749-95ec-ddcbad71af9e"), "👨‍🚀 Trung tâm chỉ huy đang chờ bạn phản hồi! 🤖 Techie đã định vị lộ trình. Hãy vào học bài đầu để khởi động chuyến bay!", null, null, null },
                    { new Guid("6920fe84-a5c0-4370-9d55-83d36db229c2"), new DateTime(2025, 5, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "vi-VN", new Guid("c182d087-fe4f-413c-ae1c-0042899e5b16"), "🌞 Dậy chưa đó? Techie nhắc nhẹ nè! Nhớ dành chút thời gian hôm nay để vào học cùng Techie nha – xu vẫn đang rơi đều, quà vẫn đang chờ bạn gom!", null, null, null },
                    { new Guid("6ac19ab3-7054-48ac-b280-092e846e2b72"), new DateTime(2025, 5, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "en-US", new Guid("1c23bddb-426a-4749-95ec-ddcbad71af9e"), "👨‍🚀 The command center is waiting for your response! 🤖 Techie has set the learning path. Start your journey by taking the first lesson!", null, null, null },
                    { new Guid("6d748f13-16b4-4075-8c20-33f6a641cbbd"), new DateTime(2025, 5, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "vi-VN", new Guid("29fd645f-4e60-4c0a-8266-d24c9854310e"), "🚨 Cảnh báo từ trung tâm FSEL Store: Có người vừa đổi quà trước bạn 1 bước! Coin bạn vẫn còn... nhưng quà thì không chắc 😬", null, null, null },
                    { new Guid("7c1b567d-1ced-4f0d-9158-cc291f9c3f86"), new DateTime(2025, 5, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "vi-VN", new Guid("2d4af96c-5344-42f3-b31f-584fa05adee2"), "⚠️ Đội bạn đã đến sao Thưởng Quà! Techie nhắc: Hãy vào học để kịp lấy coin và ghé FSEL Store trước khi kho báu đóng lại!", null, null, null },
                    { new Guid("7f211f2f-4fff-4f9f-b22b-43af2351fd9b"), new DateTime(2025, 5, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "fr-FR", new Guid("c2a40d5d-6735-40cb-a090-193aa93bcca2"), "😅 Le groupe entier a terminé la leçon 4, et vous ? Techie a réservé une place pour vous étudier avec eux. Dépêche-toi avant que l'ambiance du groupe d'étude ne baisse !", null, null, null },
                    { new Guid("8067655c-e9e4-4e64-9e99-3ae58ccc8e74"), new DateTime(2025, 5, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "en-US", new Guid("f398551d-cd3a-4641-bc82-473d718df074"), "👀 Techie is checking VAR: you haven’t studied any lessons today! VAR confirms: coins are being missed, and the Mystery Bag opportunity is waiting for you!", null, null, null },
                    { new Guid("808ed0b9-d93d-4350-8cd0-85c6e5e633e9"), new DateTime(2025, 5, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "en-US", new Guid("23990ecc-834f-44b7-a7a5-a03baa703b32"), "📊 AI predicts you'll study today… If correct, Techie will reward you with 160 coins. If wrong… Techie will be sad again 😞", null, null, null },
                    { new Guid("825b0ef3-be07-4188-9e5a-f0ebf3aa3c14"), new DateTime(2025, 5, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "vi-VN", new Guid("e10e8e9e-a132-41f2-b9e8-5a6ec055be2e"), "⏳ Đừng để hôm nay trôi qua mà chưa chọn khóa học! Học sớm hơn = nhận coin sớm hơn! Bắt đầu từ bài đầu tiên ngay nhé!", null, null, null },
                    { new Guid("82aee582-3c40-47d9-94f1-de8a5e23ab87"), new DateTime(2025, 5, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "vi-VN", new Guid("3df9fae4-6879-4196-b70e-4776b65fc51a"), "🤪 Techie tẻn tẻn khắp vũ trụ đi tìm bạn học tiếp đây nè! Không thấy bạn đâu hết trơn, vào học lẹ đi để tớ còn phát xu!", null, null, null },
                    { new Guid("896b0614-0a2b-4ba2-a0fa-3e0d96ca1187"), new DateTime(2025, 5, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "fr-FR", new Guid("8607c5e0-15e4-4a52-9312-ead10428f8c8"), "✨ 8386, c'est la prospérité et la richesse, et étudier mène à des pièces et des récompenses ! Chaque leçon d'aujourd'hui vous rapprochera des grands prix du FSEL Store !", null, null, null },
                    { new Guid("900e0318-dadf-4bba-8ca7-b12701e5c21e"), new DateTime(2025, 5, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "fr-FR", new Guid("e10e8e9e-a132-41f2-b9e8-5a6ec055be2e"), "⏳ Ne laissez pas passer aujourd'hui sans avoir choisi un cours ! Étudier plus tôt = obtenir des pièces plus tôt ! Commencez dès maintenant par la première leçon !", null, null, null },
                    { new Guid("a0ac924f-d399-4e1c-a3e2-c645b685606e"), new DateTime(2025, 5, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "fr-FR", new Guid("c15a5dd2-c897-464f-9f1c-5aa8db6cae40"), "Tu manges régulièrement, mais laisses tes leçons de côté ? 😅 Techie voit que tu as le potentiel pour échanger contre un iPhone si tu étudies régulièrement. Commence à étudier maintenant, belle !", null, null, null },
                    { new Guid("abacd77f-1a6b-47a9-8414-259d6fabd929"), new DateTime(2025, 5, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "fr-FR", new Guid("0b16fc25-ec1d-4677-84a3-6761afa2bb03"), "🌌 Techie : \"Quelle planète veux-tu explorer aujourd'hui ?\" Chaque leçon est une nouvelle planète. Choisis une leçon et commence ton voyage !", null, null, null },
                    { new Guid("abf26b88-83f0-42d4-af8f-a7fce0994d6c"), new DateTime(2025, 5, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "vi-VN", new Guid("b2de4124-4b28-4120-8156-ddef65cde6bd"), "🤖 Techie: \"Chắc bạn bị bắt cóc khỏi vũ trụ học mất rồi?\" Nếu bạn còn nhớ mật khẩu… vào học bài đầu tiên để nhận lại quyền truy cập vũ trụ nhé!", null, null, null },
                    { new Guid("b3da3ba3-d8ce-40d7-a318-d9a9422cbb88"), new DateTime(2025, 5, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "en-US", new Guid("c2a40d5d-6735-40cb-a090-193aa93bcca2"), "😅 The whole group has completed lesson 4, how about you? Techie has saved a spot for you to study with them. Hurry up before the study group mood drops!", null, null, null },
                    { new Guid("b514dbff-efdc-4256-b935-20f3bdaf1ab4"), new DateTime(2025, 5, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "fr-FR", new Guid("3df9fae4-6879-4196-b70e-4776b65fc51a"), "🤪 Techie parcourt l'univers pour te retrouver et continuer la leçon ! Je ne te trouve nulle part, dépêche-toi de commencer à étudier pour que je puisse te donner des pièces !", null, null, null },
                    { new Guid("c0b6de90-4362-4178-98b8-c5225c2d3e9c"), new DateTime(2025, 5, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "vi-VN", new Guid("23990ecc-834f-44b7-a7a5-a03baa703b32"), "📊 AI dự đoán bạn sẽ học bài hôm nay …Nếu đúng, Techie sẽ thưởng bạn 160 coin. Nếu sai… Techie sẽ buồn thêm lần nữa 😞", null, null, null },
                    { new Guid("c56c0145-dec9-4b24-9bbd-5680eb862ead"), new DateTime(2025, 5, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "fr-FR", new Guid("5d324b4a-d608-4e49-9992-b6703c534550"), "🌼 Tu es du genre relax, probablement en train de faire défiler TikTok maintenant ? Après avoir scrollé, étudie une leçon avec Techie et puis détends-toi !", null, null, null },
                    { new Guid("cce4997d-c0cd-44d9-bec5-1b2dce09c5df"), new DateTime(2025, 5, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "vi-VN", new Guid("8607c5e0-15e4-4a52-9312-ead10428f8c8"), "✨ 8386 là phát tài phát lộc, còn học là phát xu phát quà! Mỗi bài học hôm nay sẽ giúp bạn tiến gần hơn tới phần thưởng khủng từ FSEL Store!", null, null, null },
                    { new Guid("d000ca20-654a-43d9-b112-3ca383e4d0c3"), new DateTime(2025, 5, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "fr-FR", new Guid("921cdf08-0201-4a3f-a191-d415c4101018"), "👀 Techie vérifie VAR : tu n'as pas étudié de leçon aujourd'hui ! VAR confirme : des pièces sont manquées, et l'opportunité du Sac Mystère t'attend !", null, null, null },
                    { new Guid("e211ceea-672b-4f3d-a397-9b197a816f36"), new DateTime(2025, 5, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "en-US", new Guid("921cdf08-0201-4a3f-a191-d415c4101018"), "👀 Techie is checking VAR: you haven’t studied any lessons today! VAR confirms: coins are being missed, and the Mystery Bag opportunity is waiting for you!", null, null, null },
                    { new Guid("e2c95722-0baa-4b4f-a46f-8f2aca355813"), new DateTime(2025, 5, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "vi-VN", new Guid("f398551d-cd3a-4641-bc82-473d718df074"), "👀 Techie đang check VAR: bạn hôm nay chưa học bài nào luôn á! VAR xác nhận: coin đang bị bỏ lỡ, cơ hội bóc Túi Mù đang chờ bạn đó!", null, null, null },
                    { new Guid("e7c2e7c7-2367-4c87-9eb0-82dd261e512d"), new DateTime(2025, 5, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "vi-VN", new Guid("c2a40d5d-6735-40cb-a090-193aa93bcca2"), "😅 Cả nhóm học tới bài 4 rồi đó, còn bạn thì… Techie giữ chỗ cho bạn học cùng nè. Vào lẹ kẻo tụt mood học nhóm!", null, null, null },
                    { new Guid("efd0bb6a-93a2-4b6b-84fc-9e20cd86adaa"), new DateTime(2025, 5, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "vi-VN", new Guid("0b16fc25-ec1d-4677-84a3-6761afa2bb03"), "🌌 Techie: \"Hôm nay bạn muốn khám phá hành tinh nào?\" Mỗi bài học là một hành tinh mới. Vào chọn bài và khởi động tàu nhé!", null, null, null },
                    { new Guid("f1563bc3-ede7-4bf6-a53d-a35d2a334d99"), new DateTime(2025, 5, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "en-US", new Guid("c15a5dd2-c897-464f-9f1c-5aa8db6cae40"), "You eat regularly, but leave your lessons undone? 😅 Techie sees you have the potential to trade for an iPhone if you study regularly. Start studying now, beautiful!", null, null, null },
                    { new Guid("f39fec50-ed50-4ae6-b42a-5c02ef8444e9"), new DateTime(2025, 5, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "en-US", new Guid("2d4af96c-5344-42f3-b31f-584fa05adee2"), "⚠️ Your team has arrived at the Prize Planet! Techie reminds you: Join the lesson to get coins and visit the FSEL Store before the treasure closes!", null, null, null },
                    { new Guid("f54a34a8-254e-4112-8021-482055b4275f"), new DateTime(2025, 5, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "fr-FR", new Guid("fe46415d-4e6e-4248-b828-785f5486b41f"), "📊 L'IA prévoit que tu vas étudier aujourd'hui… Si c'est vrai, Techie te récompensera avec 160 pièces. Si c'est faux... Techie sera encore triste 😞", null, null, null },
                    { new Guid("f92b8309-b585-47c8-a762-f2d944a6314e"), new DateTime(2025, 5, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "en-US", new Guid("e10e8e9e-a132-41f2-b9e8-5a6ec055be2e"), "⏳ Don’t let today pass without choosing a course! The earlier you start studying = the sooner you get coins! Start from the first lesson now!", null, null, null },
                    { new Guid("f941d86a-925d-4180-aba4-a72a1d24042d"), new DateTime(2025, 5, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "vi-VN", new Guid("c15a5dd2-c897-464f-9f1c-5aa8db6cae40"), "Cơm thì ăn đều, mà bài thì bỏ đó? 😅 Techie thấy bạn có tiềm năng đổi iPhone nếu học đều đó. Vào học lẹ đi người đẹp!", null, null, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("0042ee0c-6d9c-445c-905d-e5e76ffb6662"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("029d8556-2d49-473f-8085-dc60568801d9"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("05880e81-f2b7-4a75-86ac-97191bde46d5"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("07f77fa5-27f7-4b55-9d1a-505e3876daa4"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("0823874f-fc87-428e-8e3d-91620693586a"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("0a75a7f0-aae1-4091-a773-9ec146634a3e"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("14641324-e185-4bab-be4c-9aaad65d9b5f"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("15f70c1e-72ad-4c88-8b5b-0f8fa7cbbdb1"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("172a6057-f27b-4194-9fba-a49801ba9f95"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("1d6b4320-f4f7-468e-bd27-8ac2c3309fc8"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("1e0f89d5-fa98-4ebb-90df-a1fe34d851a8"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("1ef2fb87-d639-4514-a5d3-8fa14ee5f7d6"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("1ef93023-c60c-4803-93f0-2b166c49cf5b"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("2171fb8f-d8ae-404b-b6a6-6191841ed706"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("244e2fc4-0bf1-4a69-a28a-0b9cd5a5ed86"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("2a0092c6-47f1-43b4-b7af-b08cad56173f"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("3a9c93c7-35a3-49a5-93ed-2fff62a91c9e"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("3d1b042a-4f7a-4d38-bcdc-af4985d62a89"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("6083c4b5-9d3a-4720-9f86-b41eec30e098"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("6920fe84-a5c0-4370-9d55-83d36db229c2"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("6ac19ab3-7054-48ac-b280-092e846e2b72"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("6d748f13-16b4-4075-8c20-33f6a641cbbd"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("7c1b567d-1ced-4f0d-9158-cc291f9c3f86"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("7f211f2f-4fff-4f9f-b22b-43af2351fd9b"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("8067655c-e9e4-4e64-9e99-3ae58ccc8e74"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("808ed0b9-d93d-4350-8cd0-85c6e5e633e9"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("825b0ef3-be07-4188-9e5a-f0ebf3aa3c14"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("82aee582-3c40-47d9-94f1-de8a5e23ab87"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("896b0614-0a2b-4ba2-a0fa-3e0d96ca1187"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("900e0318-dadf-4bba-8ca7-b12701e5c21e"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("a0ac924f-d399-4e1c-a3e2-c645b685606e"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("abacd77f-1a6b-47a9-8414-259d6fabd929"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("abf26b88-83f0-42d4-af8f-a7fce0994d6c"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("b3da3ba3-d8ce-40d7-a318-d9a9422cbb88"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("b514dbff-efdc-4256-b935-20f3bdaf1ab4"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("c0b6de90-4362-4178-98b8-c5225c2d3e9c"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("c56c0145-dec9-4b24-9bbd-5680eb862ead"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("cce4997d-c0cd-44d9-bec5-1b2dce09c5df"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("d000ca20-654a-43d9-b112-3ca383e4d0c3"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("e211ceea-672b-4f3d-a397-9b197a816f36"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("e2c95722-0baa-4b4f-a46f-8f2aca355813"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("e7c2e7c7-2367-4c87-9eb0-82dd261e512d"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("efd0bb6a-93a2-4b6b-84fc-9e20cd86adaa"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("f1563bc3-ede7-4bf6-a53d-a35d2a334d99"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("f39fec50-ed50-4ae6-b42a-5c02ef8444e9"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("f54a34a8-254e-4112-8021-482055b4275f"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("f92b8309-b585-47c8-a762-f2d944a6314e"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("f941d86a-925d-4180-aba4-a72a1d24042d"));

            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("0b16fc25-ec1d-4677-84a3-6761afa2bb03"));

            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("1c23bddb-426a-4749-95ec-ddcbad71af9e"));

            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("23990ecc-834f-44b7-a7a5-a03baa703b32"));

            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("29fd645f-4e60-4c0a-8266-d24c9854310e"));

            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("2d4af96c-5344-42f3-b31f-584fa05adee2"));

            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("3df9fae4-6879-4196-b70e-4776b65fc51a"));

            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("5d324b4a-d608-4e49-9992-b6703c534550"));

            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("8607c5e0-15e4-4a52-9312-ead10428f8c8"));

            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("921cdf08-0201-4a3f-a191-d415c4101018"));

            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("b2de4124-4b28-4120-8156-ddef65cde6bd"));

            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("c15a5dd2-c897-464f-9f1c-5aa8db6cae40"));

            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("c182d087-fe4f-413c-ae1c-0042899e5b16"));

            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("c2a40d5d-6735-40cb-a090-193aa93bcca2"));

            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("e10e8e9e-a132-41f2-b9e8-5a6ec055be2e"));

            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("f398551d-cd3a-4641-bc82-473d718df074"));

            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("fe46415d-4e6e-4248-b828-785f5486b41f"));
        }
    }
}
