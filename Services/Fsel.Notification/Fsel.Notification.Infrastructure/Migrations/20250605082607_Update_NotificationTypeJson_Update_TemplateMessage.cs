using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Notification.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_NotificationTypeJson_Update_TemplateMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("07f77fa5-27f7-4b55-9d1a-505e3876daa4"),
                column: "TemplateMessage",
                value: "😳 Aujourd'hui, tu surpasses même le pickleball en paresse ! Techie avertit : Si tu n'étudies pas aujourd'hui, tu vas vraiment tomber du classement !");

            migrationBuilder.UpdateData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("808ed0b9-d93d-4350-8cd0-85c6e5e633e9"),
                column: "TemplateMessage",
                value: "😳 Today, you're... surpassing pickleball in laziness! Techie warns: If you don’t study today, you’ll truly fall off the leaderboard!");

            migrationBuilder.UpdateData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("c0b6de90-4362-4178-98b8-c5225c2d3e9c"),
                column: "TemplateMessage",
                value: "😳 Hôm nay bạn đang… vượt mức pickleball về độ lười học! Techie cảnh báo: Không học bài hôm nay là trượt bảng vàng thật đó nhaaa!");

            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("23990ecc-834f-44b7-a7a5-a03baa703b32"),
                column: "TemplateMessage",
                value: "😳 Hôm nay bạn đang… vượt mức pickleball về độ lười học! Techie cảnh báo: Không học bài hôm nay là trượt bảng vàng thật đó nhaaa!");

            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("921cdf08-0201-4a3f-a191-d415c4101018"),
                column: "TemplateLink",
                value: "/marketplace");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("07f77fa5-27f7-4b55-9d1a-505e3876daa4"),
                column: "TemplateMessage",
                value: "📊 L'IA prévoit que tu vas étudier aujourd'hui… Si c'est vrai, Techie te récompensera avec 160 pièces. Si c'est faux... Techie sera encore triste 😞");

            migrationBuilder.UpdateData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("808ed0b9-d93d-4350-8cd0-85c6e5e633e9"),
                column: "TemplateMessage",
                value: "📊 AI predicts you'll study today… If correct, Techie will reward you with 160 coins. If wrong… Techie will be sad again 😞");

            migrationBuilder.UpdateData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("c0b6de90-4362-4178-98b8-c5225c2d3e9c"),
                column: "TemplateMessage",
                value: "📊 AI dự đoán bạn sẽ học bài hôm nay …Nếu đúng, Techie sẽ thưởng bạn 160 coin. Nếu sai… Techie sẽ buồn thêm lần nữa 😞");

            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("23990ecc-834f-44b7-a7a5-a03baa703b32"),
                column: "TemplateMessage",
                value: "📊 AI dự đoán bạn sẽ học bài hôm nay …Nếu đúng, Techie sẽ thưởng bạn 160 coin. Nếu sai… Techie sẽ buồn thêm lần nữa 😞");

            migrationBuilder.UpdateData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("921cdf08-0201-4a3f-a191-d415c4101018"),
                column: "TemplateLink",
                value: "/learn");
        }
    }
}
