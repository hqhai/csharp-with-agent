using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Fsel.Identity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_HumanAndUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("069ae2a9-2729-4905-a8fa-c6c9f9172d1d"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("1eeba88e-ed0c-48c4-8d7e-a17c5a2cc3fe"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("24ce207d-8732-4a32-83ef-c5f05805f124"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("4cfaa242-a625-4e02-86d9-862d48a413c8"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("65ff6784-21d7-4986-8394-681cf711a4f7"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("69976022-5dbb-4292-bab6-e94b6701061e"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("7753a049-ba55-4901-bf6a-ae65ff9ac8fc"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("8d89a29a-b40a-4045-b0ed-679d7a5ff990"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("c8c28631-0bc0-4166-b062-4aff0d38e70c"));

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "Humans",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.UpdateData(
                table: "Platform",
                keyColumn: "Id",
                keyValue: new Guid("856818e6-9e20-43d8-964f-5dab2ba1a355"),
                column: "CreatedDate",
                value: new DateTime(2023, 10, 17, 10, 49, 48, 284, DateTimeKind.Utc).AddTicks(9238));

            migrationBuilder.UpdateData(
                table: "Platform",
                keyColumn: "Id",
                keyValue: new Guid("c9c55ef5-01e0-4fa6-b68a-3aaea1089548"),
                column: "CreatedDate",
                value: new DateTime(2023, 10, 17, 10, 49, 48, 284, DateTimeKind.Utc).AddTicks(8941));

            migrationBuilder.UpdateData(
                table: "Platform",
                keyColumn: "Id",
                keyValue: new Guid("f144a094-3f49-4ca8-8f1e-7234289dd1a5"),
                column: "CreatedDate",
                value: new DateTime(2023, 10, 17, 10, 49, 48, 284, DateTimeKind.Utc).AddTicks(9289));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "Humans",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "Discription", "IsDeleted", "Name", "NormalizedName", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[,]
                {
                    { new Guid("069ae2a9-2729-4905-a8fa-c6c9f9172d1d"), null, new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Student", "Student", null, null, null },
                    { new Guid("1eeba88e-ed0c-48c4-8d7e-a17c5a2cc3fe"), null, new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Teacher", "Teacher", null, null, null },
                    { new Guid("24ce207d-8732-4a32-83ef-c5f05805f124"), null, new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Parent", "Parent", null, null, null },
                    { new Guid("4cfaa242-a625-4e02-86d9-862d48a413c8"), null, new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "TeacherLive", "TeacherLive", null, null, null },
                    { new Guid("65ff6784-21d7-4986-8394-681cf711a4f7"), null, new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Moderator", "Moderator", null, null, null },
                    { new Guid("69976022-5dbb-4292-bab6-e94b6701061e"), null, new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Admin", "Admin", null, null, null },
                    { new Guid("7753a049-ba55-4901-bf6a-ae65ff9ac8fc"), null, new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Guest", "Guest", null, null, null },
                    { new Guid("8d89a29a-b40a-4045-b0ed-679d7a5ff990"), null, new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "CSO", "CSO", null, null, null },
                    { new Guid("c8c28631-0bc0-4166-b062-4aff0d38e70c"), null, new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "MasterAdmin", "MasterAdmin", null, null, null }
                });

            migrationBuilder.UpdateData(
                table: "Platform",
                keyColumn: "Id",
                keyValue: new Guid("856818e6-9e20-43d8-964f-5dab2ba1a355"),
                column: "CreatedDate",
                value: new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "Platform",
                keyColumn: "Id",
                keyValue: new Guid("c9c55ef5-01e0-4fa6-b68a-3aaea1089548"),
                column: "CreatedDate",
                value: new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "Platform",
                keyColumn: "Id",
                keyValue: new Guid("f144a094-3f49-4ca8-8f1e-7234289dd1a5"),
                column: "CreatedDate",
                value: new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
