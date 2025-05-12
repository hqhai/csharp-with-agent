using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Fsel.Notification.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_Noti_AddCoinBuyCourse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "NotificationTypes",
                columns: new[] { "Id", "Content", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "Icon", "IsDeleted", "Priority", "TemplateLink", "TemplateMessage", "Type", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[] { new Guid("ac71e737-ca8d-4a05-9c0b-0a83edf53483"), "AddCoinBuyCourse", new DateTime(2025, 2, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, 1, "", "Chúc mừng! Bạn vừa bỏ túi {0} xu nhờ đăng ký thành công khóa học FSEL {1} tháng. Hãy chuẩn bị sẵn sàng cho hành trình học tập trong vòng {1} tháng tới nhé", "Text", null, null, null });

            migrationBuilder.InsertData(
                table: "NotificationTypeTranslations",
                columns: new[] { "Id", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "IsDeleted", "Language", "NotificationTypeId", "TemplateMessage", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[,]
                {
                    { new Guid("1072c194-d441-49a2-8126-684abe00bcb5"), new DateTime(2025, 2, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "fr-FR", new Guid("ac71e737-ca8d-4a05-9c0b-0a83edf53483"), "Félicitations ! Vous venez de gagner {0} pièces en vous inscrivant avec succès au cours FSEL de {1} mois. Préparez-vous pour un voyage d'apprentissage passionnant durant les {1} prochains mois !", null, null, null },
                    { new Guid("bc54c2ed-5127-491f-ad88-e5beb78bf2db"), new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "vi-VN", new Guid("ac71e737-ca8d-4a05-9c0b-0a83edf53483"), "Chúc mừng! Bạn vừa bỏ túi {0} xu nhờ đăng ký thành công khóa học FSEL {1} tháng. Hãy chuẩn bị sẵn sàng cho hành trình học tập trong vòng {1} tháng tới nhé", null, null, null },
                    { new Guid("f77772c8-9361-49d3-9325-36e8f03476ed"), new DateTime(2025, 2, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "en-US", new Guid("ac71e737-ca8d-4a05-9c0b-0a83edf53483"), "Congratulations! You’ve just earned {0} coins by successfully enrolling in the FSEL {1}-month course. Get ready for an exciting learning journey over the next {1} months!", null, null, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("1072c194-d441-49a2-8126-684abe00bcb5"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("bc54c2ed-5127-491f-ad88-e5beb78bf2db"));

            migrationBuilder.DeleteData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("f77772c8-9361-49d3-9325-36e8f03476ed"));

            migrationBuilder.DeleteData(
                table: "NotificationTypes",
                keyColumn: "Id",
                keyValue: new Guid("ac71e737-ca8d-4a05-9c0b-0a83edf53483"));
        }
    }
}
