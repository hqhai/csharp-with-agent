using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Notification.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateNotificationTypeV11 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("063795cd-d905-4097-8c3b-979c6830f5b0"),
                column: "NotificationTypeId",
                value: new Guid("6f75034f-4d9b-4471-b5a4-a226979b5ab3"));

            migrationBuilder.UpdateData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("6a98a753-d5d0-4296-a082-6c9d2df66a70"),
                column: "NotificationTypeId",
                value: new Guid("6f75034f-4d9b-4471-b5a4-a226979b5ab3"));

            migrationBuilder.UpdateData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("d114228f-71cb-4607-b50d-b0992e28c270"),
                column: "NotificationTypeId",
                value: new Guid("6f75034f-4d9b-4471-b5a4-a226979b5ab3"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("063795cd-d905-4097-8c3b-979c6830f5b0"),
                column: "NotificationTypeId",
                value: new Guid("a99bfaae-4139-4766-9405-ef24332fb51a"));

            migrationBuilder.UpdateData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("6a98a753-d5d0-4296-a082-6c9d2df66a70"),
                column: "NotificationTypeId",
                value: new Guid("a99bfaae-4139-4766-9405-ef24332fb51a"));

            migrationBuilder.UpdateData(
                table: "NotificationTypeTranslations",
                keyColumn: "Id",
                keyValue: new Guid("d114228f-71cb-4607-b50d-b0992e28c270"),
                column: "NotificationTypeId",
                value: new Guid("a99bfaae-4139-4766-9405-ef24332fb51a"));
        }
    }
}
