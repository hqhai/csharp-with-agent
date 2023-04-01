// Copyright (c) Atlantic. All rights reserved.

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Identity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserTokenTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            ArgumentNullException.ThrowIfNull(migrationBuilder);
            migrationBuilder.DeleteData(
                table: "aspnetroles",
                keyColumn: "id",
                keyValue: "069ae2a9-2729-4905-a8fa-c6c9f9172d1d");

            migrationBuilder.DeleteData(
                table: "aspnetroles",
                keyColumn: "id",
                keyValue: "1eeba88e-ed0c-48c4-8d7e-a17c5a2cc3fe");

            migrationBuilder.DeleteData(
                table: "aspnetroles",
                keyColumn: "id",
                keyValue: "24ce207d-8732-4a32-83ef-c5f05805f124");

            migrationBuilder.DeleteData(
                table: "aspnetroles",
                keyColumn: "id",
                keyValue: "69976022-5dbb-4292-bab6-e94b6701061e");

            migrationBuilder.DeleteData(
                table: "aspnetroles",
                keyColumn: "id",
                keyValue: "8d89a29a-b40a-4045-b0ed-679d7a5ff990");

            migrationBuilder.DeleteData(
                table: "aspnetroles",
                keyColumn: "id",
                keyValue: "c8c28631-0bc0-4166-b062-4aff0d38e70c");

            migrationBuilder.DropColumn(
                name: "RefreshToken",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "RefreshTokenExpiryTime",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "AspNetUserTokens",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RefreshToken",
                table: "AspNetUserTokens",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RefreshTokenExpiryTime",
                table: "AspNetUserTokens",
                type: "datetime2",
                nullable: true);

            //migrationBuilder.InsertData(
            //    table: "AspNetRoles",
            //    columns: new[] { "Id", "ConcurrencyStamp", "Discriminator", "Discription", "Name", "NormalizedName" },
            //    values: new object[,]
            //    {
            //        { "0c7fee86-6d80-4b27-980a-282dc90fc9b7", null, "Role", null, "Student", "Student" },
            //        { "b743dad9-7538-4b5e-bcbe-52997cda0a17", null, "Role", null, "Admin", "Admin" },
            //        { "c2980f2a-1e8d-409e-bd18-a4f724a6f074", null, "Role", null, "CSO", "CSO" },
            //        { "c9bd5d42-78f3-4e36-9c6e-55b0ac763f03", null, "Role", null, "MasterAdmin", "MasterAdmin" },
            //        { "d119ed1b-6810-415d-a69f-8a1c0aac6ab6", null, "Role", null, "Teacher", "Teacher" },
            //        { "f0d61936-5fb8-4ad2-8f05-35a247cb6c82", null, "Role", null, "Parent", "Parent" }
            //    });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            ArgumentNullException.ThrowIfNull(migrationBuilder);
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "0c7fee86-6d80-4b27-980a-282dc90fc9b7");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "b743dad9-7538-4b5e-bcbe-52997cda0a17");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "c2980f2a-1e8d-409e-bd18-a4f724a6f074");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "c9bd5d42-78f3-4e36-9c6e-55b0ac763f03");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "d119ed1b-6810-415d-a69f-8a1c0aac6ab6");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "f0d61936-5fb8-4ad2-8f05-35a247cb6c82");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "AspNetUserTokens");

            migrationBuilder.DropColumn(
                name: "RefreshToken",
                table: "AspNetUserTokens");

            migrationBuilder.DropColumn(
                name: "RefreshTokenExpiryTime",
                table: "AspNetUserTokens");

            migrationBuilder.AddColumn<string>(
                name: "RefreshToken",
                table: "AspNetUsers",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RefreshTokenExpiryTime",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            //migrationBuilder.InsertData(
            //    table: "AspNetRoles",
            //    columns: new[] { "Id", "ConcurrencyStamp", "Discriminator", "Discription", "Name", "NormalizedName" },
            //    values: new object[,]
            //    {
            //        { "069ae2a9-2729-4905-a8fa-c6c9f9172d1d", null, "Role", null, "Student", "Student" },
            //        { "1eeba88e-ed0c-48c4-8d7e-a17c5a2cc3fe", null, "Role", null, "Teacher", "Teacher" },
            //        { "24ce207d-8732-4a32-83ef-c5f05805f124", null, "Role", null, "Parent", "Parent" },
            //        { "69976022-5dbb-4292-bab6-e94b6701061e", null, "Role", null, "Admin", "Admin" },
            //        { "8d89a29a-b40a-4045-b0ed-679d7a5ff990", null, "Role", null, "CSO", "CSO" },
            //        { "c8c28631-0bc0-4166-b062-4aff0d38e70c", null, "Role", null, "MasterAdmin", "MasterAdmin" }
            //    });
        }
    }
}
