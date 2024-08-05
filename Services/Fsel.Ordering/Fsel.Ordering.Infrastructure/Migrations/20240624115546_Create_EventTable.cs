using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Fsel.Ordering.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Create_EventTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Events",
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
                    Code = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ImagePathsStr = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EventTranslations",
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
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Language = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventTranslations_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PackageEvents",
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
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PriceMonth = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DayBonus = table.Column<int>(type: "int", nullable: false),
                    MonthBonus = table.Column<int>(type: "int", nullable: false),
                    Suggest = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PackageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackageEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PackageEvents_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PackageEvents_Packages_PackageId",
                        column: x => x.PackageId,
                        principalTable: "Packages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Events",
                columns: new[] { "Id", "Code", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "Description", "EndDate", "ImagePathsStr", "IsDefault", "IsDeleted", "Name", "StartDate", "Status", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[] { new Guid("2371c9af-01a6-489b-8399-0e2a13db0646"), "DEF", new DateTime(2024, 6, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Tặng thêm thời gian học khi mua gói 6 tháng, 12 tháng nhân sự kiện ra mắt FSEL.", null, "[ \"https://s3-sgn10.fptcloud.com/fsel-public/Images/BannerEvent_7538_1719195644832.jpg\" ]", true, false, "Sự kiện mặc định", null, "Active", null, null, null });

            migrationBuilder.InsertData(
                table: "EventTranslations",
                columns: new[] { "Id", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "Description", "EventId", "IsDeleted", "Language", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[,]
                {
                    { new Guid("7ec4ded7-afd4-4c84-8b71-12c8d89afabd"), new DateTime(2024, 6, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Tặng thêm thời gian học khi mua gói 6 tháng, 12 tháng nhân sự kiện ra mắt FSEL.", new Guid("2371c9af-01a6-489b-8399-0e2a13db0646"), false, "vi-VN", null, null, null },
                    { new Guid("a69de45a-a796-4a17-a18d-565e1043bfef"), new DateTime(2024, 6, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Bénéficiez de temps d'étude supplémentaire en achetant des forfaits de 6 ou 12 mois lors de l'événement de lancement du FSEL", new Guid("2371c9af-01a6-489b-8399-0e2a13db0646"), false, "fr-FR", null, null, null },
                    { new Guid("c64d896b-4a94-4ac3-97ba-28ffef798628"), new DateTime(2024, 6, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Get extra study time when purchasing 6-month or 12-month packages during the FSEL launch event", new Guid("2371c9af-01a6-489b-8399-0e2a13db0646"), false, "en-US", null, null, null }
                });

            migrationBuilder.InsertData(
                table: "PackageEvents",
                columns: new[] { "Id", "CreatedDate", "CreatedFullName", "CreatedUserId", "DayBonus", "DeletedDate", "DeletedFullName", "DeletedUserId", "EventId", "IsDeleted", "MonthBonus", "PackageId", "Price", "PriceMonth", "Suggest", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[,]
                {
                    { new Guid("2496f21a-7e62-41a3-a294-89674a411e03"), new DateTime(2024, 6, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), 15, null, null, null, new Guid("2371c9af-01a6-489b-8399-0e2a13db0646"), false, 1, new Guid("daa6fc87-6461-49d4-b3a5-c9e4cc30bc59"), 2400000m, 320000m, "BestSeller", null, null, null },
                    { new Guid("7ec4ded7-afd4-4c84-8b71-12c8d89afabd"), new DateTime(2024, 6, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), 0, null, null, null, new Guid("2371c9af-01a6-489b-8399-0e2a13db0646"), false, 0, new Guid("42d7ddb2-9f36-4f86-badc-67dc16bb722b"), 500000m, 500000m, null, null, null, null },
                    { new Guid("ebe09800-afd8-4621-a490-7f1fecf0e54c"), new DateTime(2024, 6, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), 0, null, null, null, new Guid("2371c9af-01a6-489b-8399-0e2a13db0646"), false, 3, new Guid("d13ee4ab-785a-425c-bd70-b74b61df42eb"), 3600000m, 240000m, "Recommend", null, null, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventTranslations_EventId",
                table: "EventTranslations",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_PackageEvents_EventId",
                table: "PackageEvents",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_PackageEvents_PackageId",
                table: "PackageEvents",
                column: "PackageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventTranslations");

            migrationBuilder.DropTable(
                name: "PackageEvents");

            migrationBuilder.DropTable(
                name: "Events");
        }
    }
}
