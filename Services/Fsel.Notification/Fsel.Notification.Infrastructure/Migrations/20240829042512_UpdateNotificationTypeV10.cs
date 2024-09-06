using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Fsel.Notification.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateNotificationTypeV10 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NotificationTypeTranslations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedFullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UpdatedFullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DeletedFullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    TemplateMessage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NotificationTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Language = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationTypeTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificationTypeTranslations_NotificationTypes_NotificationTypeId",
                        column: x => x.NotificationTypeId,
                        principalTable: "NotificationTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "NotificationTypeTranslations",
                columns: new[] { "Id", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "IsDeleted", "Language", "NotificationTypeId", "TemplateMessage", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[,]
                {
                    { new Guid("02dc26ac-2d45-4983-b0e8-15b4c7cb8b90"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "vi-VN", new Guid("810c70ca-4f3c-4d02-a194-708fa45d42ea"), "{0} vừa bình luận bài viết của bạn.", null, null, null },
                    { new Guid("0489ba9d-222f-4bea-ae35-3d8ac36cb35f"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "fr-FR", new Guid("810c70ca-4f3c-4d02-a194-708fa45d42ea"), "{0} vient de commenter votre publication.", null, null, null },
                    { new Guid("063795cd-d905-4097-8c3b-979c6830f5b0"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "en-US", new Guid("a99bfaae-4139-4766-9405-ef24332fb51a"), "Yay! You've successfully changed your level to the {0} course! Let's join Techie and Fsel to start this exciting learning journey now!", null, null, null },
                    { new Guid("0d199223-e152-4a39-9b22-c0a510033e14"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "vi-VN", new Guid("9e983172-dba2-4ca7-b602-372c011ecb99"), "{0} vừa trả lời bình luận của bạn.", null, null, null },
                    { new Guid("1dc3d61c-00f1-434a-9700-5a1adc423745"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "fr-FR", new Guid("9da28757-2d28-40cb-b88c-cc51fc15bb43"), "Félicitations pour avoir atteint le top {0} du classement !", null, null, null },
                    { new Guid("39ddf48b-b3bb-4898-91e0-54451a4accd0"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "vi-VN", new Guid("9da28757-2d28-40cb-b88c-cc51fc15bb43"), "Chúc mừng bạn đã đạt top {0} trên Bảng xếp hạng!", null, null, null },
                    { new Guid("48853c68-df32-4ba2-af20-73d47260655d"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "fr-FR", new Guid("810c70ca-4f3c-4d02-a491-708fa45d42ea"), "{0} a aimé votre publication !", null, null, null },
                    { new Guid("561ea827-f02c-49a8-9ea7-240994cfb8cd"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "vi-VN", new Guid("75540141-a6d9-409d-a3ce-90c9a7577e8a"), "Bài viết của bạn trong {0} đã bị từ chối phê duyệt do vi phạm tiêu chuẩn cộng đồng của FSEL. Vui lòng thử lại!", null, null, null },
                    { new Guid("5a5954c6-b3f4-4018-8bf2-7134e09ea540"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "fr-FR", new Guid("69cc9e39-82d5-4d1a-81b9-8c97ec54bfad"), "Votre publication a été approuvée. Cliquez pour voir plus de détails.", null, null, null },
                    { new Guid("61a570d9-f58d-4278-863f-120680ef789b"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "en-US", new Guid("9da28757-2d28-40cb-b88c-cc51fc15bb43"), "Congratulations on reaching top {0} on the Leaderboard!", null, null, null },
                    { new Guid("66de6e9c-0c63-4bad-b5e4-c52cafe2bb7f"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "vi-VN", new Guid("69cc9e39-82d5-4d1a-81b9-8c97ec54bfad"), "Bài đăng của bạn đã được phê duyệt. Nhấn để xem chi tiết", null, null, null },
                    { new Guid("678236d5-543b-4dd1-ab99-c2caa99a62af"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "en-US", new Guid("810c70ca-4f3c-4d02-a194-708fa45d42ea"), "{0} has just commented on your post.", null, null, null },
                    { new Guid("6a98a753-d5d0-4296-a082-6c9d2df66a70"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "fr-FR", new Guid("a99bfaae-4139-4766-9405-ef24332fb51a"), "Yay! Vous avez réussi à changer votre niveau pour le cours {0} ! Rejoignons Techie et Fsel pour commencer ce voyage d'apprentissage passionnant maintenant", null, null, null },
                    { new Guid("72c38c36-fc6b-4ed6-a8e6-fc711903b5af"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "fr-FR", new Guid("27007edb-25d8-493e-84d8-85f6f9e165b2"), "Votre commentaire sur la publication de {0} a été supprimé car il enfreignait les normes de la communauté FSEL.", null, null, null },
                    { new Guid("760bcc15-6445-4302-b59f-07a3d4c5337a"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "en-US", new Guid("59942527-ca1f-4f18-a66b-cdcfe87b317e"), "Your submission has been graded by FSEL's AI. Click to view the results.", null, null, null },
                    { new Guid("77c8859e-37c0-4ce9-8bf0-d9aa2cec1d43"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "en-US", new Guid("9e983172-dba2-4ca7-b602-372c011ecb99"), "{0} has just replied to your comment.", null, null, null },
                    { new Guid("782036f2-e5e3-4a36-b8ea-484c7c1db06c"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "en-US", new Guid("810c70ca-4f3c-4d02-a491-708fa45d42ea"), "{0} has liked your post!", null, null, null },
                    { new Guid("876495b2-1756-4cb3-8a68-2261221a89ad"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "vi-VN", new Guid("27007edb-25d8-493e-84d8-85f6f9e165b2"), "Bình luận của bạn trong bài viết của {0} đã bị gỡ do vi phạm tiêu chuẩn cộng đồng của FSEL.", null, null, null },
                    { new Guid("973b9f47-3bb6-4ecc-8a4f-e6482c6fb80e"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "vi-VN", new Guid("cd8a1edb-029f-4b2d-9725-c796e6fd7e5b"), "Bạn đã mua gói {0} thành công. Hãy bắt đầu học nào!", null, null, null },
                    { new Guid("9bfd2b9d-eb69-4a3b-8788-6526e1b4d784"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "fr-FR", new Guid("57c709a6-03bd-4a3f-ad34-f9449587e1df"), "Votre publication dans {0} a été supprimée pour non-respect des normes de la communauté FSEL. Veuillez réessayer !", null, null, null },
                    { new Guid("a5609e92-e166-46ec-ad23-2c0222fcb351"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "en-US", new Guid("69cc9e39-82d5-4d1a-81b9-8c97ec54bfad"), "Your post has been approved. Click to view more details.", null, null, null },
                    { new Guid("abab8049-4c5d-44c7-93dc-bca5416fc87e"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "fr-FR", new Guid("59942527-ca1f-4f18-a66b-cdcfe87b317e"), "Votre soumission a été notée par l'IA de FSEL. Cliquez pour voir les résultats.", null, null, null },
                    { new Guid("d095d2c0-2266-4765-b4d4-2da0c685336f"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "en-US", new Guid("57c709a6-03bd-4a3f-ad34-f9449587e1df"), "Your post in {0} has been removed for violating FSEL's community standards. Please try again!", null, null, null },
                    { new Guid("d114228f-71cb-4607-b50d-b0992e28c270"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "vi-VN", new Guid("a99bfaae-4139-4766-9405-ef24332fb51a"), "Yay! Bạn đã đổi trình độ sang khóa học {0} thành công! Hãy cùng Techie và Fsel bắt đầu hành trình học tập thú vị ngay thôi nào!", null, null, null },
                    { new Guid("d8977047-e977-4772-a3e0-c20cb0bc76e3"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "vi-VN", new Guid("59942527-ca1f-4f18-a66b-cdcfe87b317e"), "Bài đăng của bạn đã được chấm bởi hệ thống AI ChatGPT. Nhấn để xem chi tiết", null, null, null },
                    { new Guid("dbfdb359-cd76-443d-94c1-c73888957d56"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "fr-FR", new Guid("9e983172-dba2-4ca7-b602-372c011ecb99"), "{0} vient de répondre à votre commentaire.", null, null, null },
                    { new Guid("dc5dd6e3-41d5-4221-b5ff-04035926cf7d"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "vi-VN", new Guid("810c70ca-4f3c-4d02-a491-708fa45d42ea"), "{0} đã thích bài viết của bạn.", null, null, null },
                    { new Guid("de49fb73-928f-418c-a8bc-58d843ff0d42"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "en-US", new Guid("75540141-a6d9-409d-a3ce-90c9a7577e8a"), "Your post in {0} has been denied due to violating FSEL's community standards. Please try again!", null, null, null },
                    { new Guid("dff468cf-a150-4685-b7ba-fa2d4eeb2b05"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "vi-VN", new Guid("57c709a6-03bd-4a3f-ad34-f9449587e1df"), "Bài viết của bạn trong {0} đã bị gỡ do vi phạm tiêu chuẩn cộng đồng của FSEL. Vui lòng thử lại!", null, null, null },
                    { new Guid("e43e20b2-ed0a-4353-9e44-faa7ad0f2bf0"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "fr-FR", new Guid("75540141-a6d9-409d-a3ce-90c9a7577e8a"), "Votre publication dans {0} a été refusée en raison de la violation des normes de la communauté FSEL. Veuillez réessayer !", null, null, null },
                    { new Guid("e8d53f67-68ee-4352-b134-ab47d6e9768c"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "en-US", new Guid("cd8a1edb-029f-4b2d-9725-c796e6fd7e5b"), "You have successfully purchased the {0} package. Let's start learning!", null, null, null },
                    { new Guid("efb6b960-c668-4643-b433-93142da84ad3"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "en-US", new Guid("27007edb-25d8-493e-84d8-85f6f9e165b2"), "Your comment on {0}'s post has been removed because it violated FSEL's community standards.", null, null, null },
                    { new Guid("f5560aac-3ac4-4a88-9d84-a82800e1b149"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "fr-FR", new Guid("cd8a1edb-029f-4b2d-9725-c796e6fd7e5b"), "Vous avez acheté avec succès le forfait {0}. Commençons à apprendre !", null, null, null }
                });

            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("59942527-ca1f-4f18-a66b-cdcfe87b317e"),
                column: "TemplateMessage",
                value: "Bài đăng của bạn đã được chấm bởi hệ thống AI ChatGPT. Nhấn để xem chi tiết");

            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("cd8a1edb-029f-4b2d-9725-c796e6fd7e5b"),
                column: "TemplateMessage",
                value: "Bạn đã mua gói {0} thành công. Hãy bắt đầu học nào!");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationTypeTranslations_NotificationTypeId",
                table: "NotificationTypeTranslations",
                column: "NotificationTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NotificationTypeTranslations");

            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("59942527-ca1f-4f18-a66b-cdcfe87b317e"),
                column: "TemplateMessage",
                value: "Bài viết của bạn đã được hệ thống AI ChatGPT nhận xét. Nhấn để xem chi tiết");

            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("cd8a1edb-029f-4b2d-9725-c796e6fd7e5b"),
                column: "TemplateMessage",
                value: "Bạn đã mua khóa học {0} thành công. Hãy bắt đầu học nào!");
        }
    }
}
