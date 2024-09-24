using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Fsel.Ordering.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_PackageTable_AddFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DescriptionStr",
                table: "Packages",
                newName: "IncentivesWhenPurchasing");

            migrationBuilder.AddColumn<double>(
                name: "MonthBonusNumber",
                table: "Packages",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<decimal>(
                name: "PriceMonth",
                table: "Packages",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Suggest",
                table: "Packages",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PackageTranslations",
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
                    IncentivesWhenPurchasing = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PackageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Language = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackageTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PackageTranslations_Packages_PackageId",
                        column: x => x.PackageId,
                        principalTable: "Packages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "PackageTranslations",
                columns: new[] { "Id", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "IncentivesWhenPurchasing", "IsDeleted", "Language", "PackageId", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[,]
                {
                    { new Guid("5f1edb1d-ca8b-4006-b13f-797ffea98973"), new DateTime(2024, 6, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Faire don des droits d'auteur des Éditions de l'Université de Cambridge", false, "fr-FR", new Guid("d13ee4ab-785a-425c-bd70-b74b61df42eb"), null, null, null },
                    { new Guid("81c2c7d2-9b05-485c-9659-e7f8896955a8"), new DateTime(2024, 6, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Tặng phí bản quyền của NXB Đại học Cambridge", false, "vi-VN", new Guid("42d7ddb2-9f36-4f86-badc-67dc16bb722b"), null, null, null },
                    { new Guid("86f466db-ff5a-4aa7-846d-52e25ab1f38f"), new DateTime(2024, 6, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Faire don des droits d'auteur des Éditions de l'Université de Cambridge", false, "fr-FR", new Guid("daa6fc87-6461-49d4-b3a5-c9e4cc30bc59"), null, null, null },
                    { new Guid("b8a900bd-269a-410c-b8bd-22cc32623a38"), new DateTime(2024, 6, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Free copyright fee from Cambridge University Press", false, "en-US", new Guid("d13ee4ab-785a-425c-bd70-b74b61df42eb"), null, null, null },
                    { new Guid("bb9cdf82-74aa-456b-9f8b-ac4902a908cf"), new DateTime(2024, 6, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Free copyright fee from Cambridge University Press", false, "en-US", new Guid("42d7ddb2-9f36-4f86-badc-67dc16bb722b"), null, null, null },
                    { new Guid("cc63e0ed-c6ed-480a-9f32-4952d296ef29"), new DateTime(2024, 6, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Faire don des droits d'auteur des Éditions de l'Université de Cambridge", false, "fr-FR", new Guid("42d7ddb2-9f36-4f86-badc-67dc16bb722b"), null, null, null },
                    { new Guid("dc09719c-78d6-45a3-bd11-1ee9b3fe8814"), new DateTime(2024, 6, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Tặng phí bản quyền của NXB Đại học Cambridge", false, "vi-VN", new Guid("daa6fc87-6461-49d4-b3a5-c9e4cc30bc59"), null, null, null },
                    { new Guid("ec9b607c-055e-43f6-a705-f8a0258b4136"), new DateTime(2024, 6, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Free copyright fee from Cambridge University Press", false, "en-US", new Guid("daa6fc87-6461-49d4-b3a5-c9e4cc30bc59"), null, null, null },
                    { new Guid("edb73176-e5bd-4e13-9954-7eacf5968bb8"), new DateTime(2024, 6, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Tặng phí bản quyền của NXB Đại học Cambridge", false, "vi-VN", new Guid("d13ee4ab-785a-425c-bd70-b74b61df42eb"), null, null, null }
                });

            migrationBuilder.UpdateData(
                table: "Packages",
                keyColumn: "Id",
                keyValue: new Guid("42d7ddb2-9f36-4f86-badc-67dc16bb722b"),
                columns: new[] { "CreatedDate", "IncentivesWhenPurchasing", "MonthBonusNumber", "Price", "PriceMonth", "Suggest" },
                values: new object[] { new DateTime(2024, 6, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "Tặng phí bản quyền của NXB Đại học Cambridge", 0.0, 500000m, 500000m, null });

            migrationBuilder.UpdateData(
                table: "Packages",
                keyColumn: "Id",
                keyValue: new Guid("d13ee4ab-785a-425c-bd70-b74b61df42eb"),
                columns: new[] { "Code", "CreatedDate", "IncentivesWhenPurchasing", "MonthBonusNumber", "Price", "PriceMonth", "Suggest" },
                values: new object[] { "PREMIUM", new DateTime(2024, 6, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "Tặng phí bản quyền của NXB Đại học Cambridge", 3.0, 3600000m, 240000m, "Recommend" });

            migrationBuilder.UpdateData(
                table: "Packages",
                keyColumn: "Id",
                keyValue: new Guid("daa6fc87-6461-49d4-b3a5-c9e4cc30bc59"),
                columns: new[] { "Code", "CreatedDate", "IncentivesWhenPurchasing", "MonthBonusNumber", "Price", "PriceMonth", "Suggest" },
                values: new object[] { "STANDARD", new DateTime(2024, 6, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "Tặng phí bản quyền của NXB Đại học Cambridge", 1.5, 2400000m, 320000m, "BestSeller" });

            migrationBuilder.CreateIndex(
                name: "IX_PackageTranslations_PackageId",
                table: "PackageTranslations",
                column: "PackageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PackageTranslations");

            migrationBuilder.DropColumn(
                name: "MonthBonusNumber",
                table: "Packages");

            migrationBuilder.DropColumn(
                name: "PriceMonth",
                table: "Packages");

            migrationBuilder.DropColumn(
                name: "Suggest",
                table: "Packages");

            migrationBuilder.RenameColumn(
                name: "IncentivesWhenPurchasing",
                table: "Packages",
                newName: "DescriptionStr");

            migrationBuilder.UpdateData(
                table: "Packages",
                keyColumn: "Id",
                keyValue: new Guid("42d7ddb2-9f36-4f86-badc-67dc16bb722b"),
                columns: new[] { "CreatedDate", "DescriptionStr", "Price" },
                values: new object[] { new DateTime(2023, 7, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "[{\"content\":\"Giá trên đã bao gồm Chi phí chính sách Cambridge: 150,000 VND\",\"status\":true}]", 650000m });

            migrationBuilder.UpdateData(
                table: "Packages",
                keyColumn: "Id",
                keyValue: new Guid("d13ee4ab-785a-425c-bd70-b74b61df42eb"),
                columns: new[] { "Code", "CreatedDate", "DescriptionStr", "Price" },
                values: new object[] { "BASIC", new DateTime(2023, 7, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "[{\"content\":\"Giá trên đã bao gồm Chi phí chính sách Cambridge: 150,000 VND\",\"status\":true}]", 3100000m });

            migrationBuilder.UpdateData(
                table: "Packages",
                keyColumn: "Id",
                keyValue: new Guid("daa6fc87-6461-49d4-b3a5-c9e4cc30bc59"),
                columns: new[] { "Code", "CreatedDate", "DescriptionStr", "Price" },
                values: new object[] { "BASIC", new DateTime(2023, 7, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "[{\"content\":\"Giá trên đã bao gồm Chi phí chính sách Cambridge: 150,000 VND\",\"status\":true}]", 2100000m });
        }
    }
}
