using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Notification.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class update_type_surveyreward : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("13dd85c7-014c-4a32-9392-92a46440c481"),
                column: "TemplateMessage",
                value: "Thank you for rating FSEL 5 stars! FSEL has credited {0} coins to your account. Keep up the hard work! ");

            migrationBuilder.UpdateData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("788c4de6-030b-4866-8e91-3fb0e913f670"),
                column: "TemplateMessage",
                value: "Cảm ơn bạn đã đánh giá FSEL 5 sao! FSEL đã gửi tặng {0} xu vào tài khoản của bạn. Tiếp tục học tập chăm chỉ nhé!");

            migrationBuilder.UpdateData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("8592527a-e771-4e37-a2e0-e5e5aa5ff667"),
                column: "TemplateMessage",
                value: "Merci d'avoir attribué 5 étoiles à FSEL ! FSEL a crédité {0} pièces sur votre compte. Continuez à travailler dur !");

            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("3c7f9946-3f95-4a84-b2df-7cffbf36a41c"),
                column: "TemplateMessage",
                value: "Cảm ơn bạn đã đánh giá FSEL 5 sao! FSEL đã gửi tặng {0} xu vào tài khoản của bạn. Tiếp tục học tập chăm chỉ nhé!");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("13dd85c7-014c-4a32-9392-92a46440c481"),
                column: "TemplateMessage",
                value: "Thank you for participating in the survey! We sincerely appreciate your completion of the Self-Study Language Month - Hanoi survey! As a token of our gratitude, you have received {0} FSEL COINS. Your feedback will be carefully reviewed and evaluated to continuously improve the learning experience. Thank you for your support and for being part of FSEL!  ");

            migrationBuilder.UpdateData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("788c4de6-030b-4866-8e91-3fb0e913f670"),
                column: "TemplateMessage",
                value: "Cảm ơn bạn đã tham gia khảo sát! Chúng tôi trân trọng cảm ơn bạn đã hoàn thành khảo sát Tháng Tự Học Ngoại Ngữ - Hà Nội! Để bày tỏ sự tri ân, bạn đã nhận được {0} FSEL COINS. Chúng tôi sẽ xem xét và đánh giá ý kiến của bạn để không ngừng cải thiện trải nghiệm học tập. Cảm ơn bạn đã đồng hành cùng FSEL!");

            migrationBuilder.UpdateData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("8592527a-e771-4e37-a2e0-e5e5aa5ff667"),
                column: "TemplateMessage",
                value: "Merci d'avoir participé à l'enquête ! Nous vous remercions sincèrement d'avoir complété l'enquête sur le Mois de l'Auto-apprentissage des Langues - Hanoï ! En signe de notre gratitude, vous avez reçu {0} FSEL COINS. Vos retours seront attentivement examinés et évalués afin d'améliorer continuellement l'expérience d'apprentissage. Merci pour votre soutien et votre engagement avec FSEL ! ");

            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("3c7f9946-3f95-4a84-b2df-7cffbf36a41c"),
                column: "TemplateMessage",
                value: "Cảm ơn bạn đã tham gia khảo sát! Chúng tôi trân trọng cảm ơn bạn đã hoàn thành khảo sát Tháng Tự Học Ngoại Ngữ - Hà Nội! Để bày tỏ sự tri ân, bạn đã nhận được {0} FSEL COINS. Chúng tôi sẽ xem xét và đánh giá ý kiến của bạn để không ngừng cải thiện trải nghiệm học tập. Cảm ơn bạn đã đồng hành cùng FSEL!");
        }
    }
}
