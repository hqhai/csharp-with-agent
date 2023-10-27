using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Fsel.Ordering.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Packages",
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
                    Code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DescriptionStr = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Packages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
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
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Country = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiscountPercent = table.Column<int>(type: "int", nullable: false),
                    DiscountPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CourseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PackageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Orders_Packages_PackageId",
                        column: x => x.PackageId,
                        principalTable: "Packages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Packages",
                columns: new[] { "Id", "Code", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "DescriptionStr", "IsDeleted", "Price", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[,]
                {
                    { new Guid("42d7ddb2-9f36-4f86-badc-67dc16bb722b"), "BASIC", new DateTime(2023, 6, 7, 9, 6, 15, 722, DateTimeKind.Local).AddTicks(4916), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "[{\"content\":\"B\\u00E0i gi\\u1EA3ng , b\\u00E0i t\\u1EADp t\\u00EAn n\\u1EC1n t\\u1EA3ng E-learning\",\"status\":true},{\"content\":\"Truy c\\u1EADp b\\u00E0i t\\u1EADp h\\u01B0\\u1EDBng d\\u1EABn, v\\u00E0 b\\u00E0i thi Unit\",\"status\":true},{\"content\":\"Di\\u1EC5n \\u0111\\u00E0n\",\"status\":true},{\"content\":\"Gi\\u1EA3ng vi\\u00EAn nh\\u1EADn x\\u00E9t\",\"status\":false},{\"content\":\"Truy c\\u1EADp ti\\u1EBFt h\\u1ECDc tr\\u1EF1c tuy\\u1EBFn cho k\\u1EF9 n\\u0103ng n\\u00F3i v\\u1EDBi Gi\\u1EA3ng vi\\u00EAn\",\"status\":false}]", false, 1000000m, null, null, null },
                    { new Guid("d13ee4ab-785a-425c-bd70-b74b61df42eb"), "PREMIUM", new DateTime(2023, 6, 7, 9, 6, 15, 741, DateTimeKind.Local).AddTicks(8989), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "[{\"content\":\"B\\u00E0i gi\\u1EA3ng , b\\u00E0i t\\u1EADp t\\u00EAn n\\u1EC1n t\\u1EA3ng E-learning\",\"status\":true},{\"content\":\"Truy c\\u1EADp b\\u00E0i t\\u1EADp h\\u01B0\\u1EDBng d\\u1EABn, v\\u00E0 b\\u00E0i thi Unit\",\"status\":true},{\"content\":\"Di\\u1EC5n \\u0111\\u00E0n\",\"status\":true},{\"content\":\"Gi\\u1EA3ng vi\\u00EAn nh\\u1EADn x\\u00E9t\",\"status\":true},{\"content\":\"Truy c\\u1EADp ti\\u1EBFt h\\u1ECDc tr\\u1EF1c tuy\\u1EBFn cho k\\u1EF9 n\\u0103ng n\\u00F3i v\\u1EDBi Gi\\u1EA3ng vi\\u00EAn\",\"status\":true}]", false, 10000000m, null, null, null },
                    { new Guid("daa6fc87-6461-49d4-b3a5-c9e4cc30bc59"), "STANDARD", new DateTime(2023, 6, 7, 9, 6, 15, 741, DateTimeKind.Local).AddTicks(2833), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "[{\"content\":\"B\\u00E0i gi\\u1EA3ng , b\\u00E0i t\\u1EADp t\\u00EAn n\\u1EC1n t\\u1EA3ng E-learning\",\"status\":true},{\"content\":\"Truy c\\u1EADp b\\u00E0i t\\u1EADp h\\u01B0\\u1EDBng d\\u1EABn, v\\u00E0 b\\u00E0i thi Unit\",\"status\":true},{\"content\":\"Di\\u1EC5n \\u0111\\u00E0n\",\"status\":true},{\"content\":\"Gi\\u1EA3ng vi\\u00EAn nh\\u1EADn x\\u00E9t\",\"status\":true},{\"content\":\"Truy c\\u1EADp ti\\u1EBFt h\\u1ECDc tr\\u1EF1c tuy\\u1EBFn cho k\\u1EF9 n\\u0103ng n\\u00F3i v\\u1EDBi Gi\\u1EA3ng vi\\u00EAn\",\"status\":false}]", false, 3000000m, null, null, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_PackageId",
                table: "Orders",
                column: "PackageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "Packages");
        }
    }
}
