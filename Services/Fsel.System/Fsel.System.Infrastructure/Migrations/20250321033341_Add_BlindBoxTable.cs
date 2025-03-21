using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Fsel.System.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_BlindBoxTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BlindBoxes",
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
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MileStone = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlindBoxes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BlindBoxChests",
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
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MaxOpenCount = table.Column<int>(type: "int", nullable: true),
                    OpenPrice = table.Column<int>(type: "int", nullable: false),
                    Index = table.Column<int>(type: "int", nullable: false),
                    IsLast = table.Column<bool>(type: "bit", nullable: false),
                    BlindBoxId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlindBoxChests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BlindBoxChests_BlindBoxes_BlindBoxId",
                        column: x => x.BlindBoxId,
                        principalTable: "BlindBoxes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BlindBoxUsers",
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
                    BlindBoxId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NumberOpen = table.Column<int>(type: "int", nullable: false),
                    IsWin = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlindBoxUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BlindBoxUsers_BlindBoxes_BlindBoxId",
                        column: x => x.BlindBoxId,
                        principalTable: "BlindBoxes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BlindBoxChestConfigs",
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
                    ConfigType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ImagePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Coin = table.Column<int>(type: "int", nullable: true),
                    Percentage = table.Column<int>(type: "int", nullable: false),
                    BlindBoxChestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlindBoxChestConfigs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BlindBoxChestConfigs_BlindBoxChests_BlindBoxChestId",
                        column: x => x.BlindBoxChestId,
                        principalTable: "BlindBoxChests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BlindBoxHistories",
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
                    BlindBoxChestConfigId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Coin = table.Column<int>(type: "int", nullable: true),
                    IsPiece = table.Column<bool>(type: "bit", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlindBoxHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BlindBoxHistories_BlindBoxChestConfigs_BlindBoxChestConfigId",
                        column: x => x.BlindBoxChestConfigId,
                        principalTable: "BlindBoxChestConfigs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "BlindBoxes",
                columns: new[] { "Id", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "EndDate", "IsDeleted", "MileStone", "Name", "StartDate", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[] { new Guid("ff62d2c7-f1e2-48a4-b613-17b4ebd5cce7"), new DateTime(2025, 3, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khoa Ozil", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, new DateTime(2025, 8, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 300, "Blind Box", new DateTime(2025, 4, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, null });

            migrationBuilder.InsertData(
                table: "BlindBoxChests",
                columns: new[] { "Id", "BlindBoxId", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "Index", "IsDeleted", "IsLast", "MaxOpenCount", "Name", "OpenPrice", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[,]
                {
                    { new Guid("1632fece-f46e-4ce5-a374-13c74f61ae58"), new Guid("ff62d2c7-f1e2-48a4-b613-17b4ebd5cce7"), new DateTime(2025, 3, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khoa Ozil", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 3, false, false, 30, "Rương 3", 90, null, null, null },
                    { new Guid("374abb96-b10e-4fe7-82a7-79b20ec37548"), new Guid("ff62d2c7-f1e2-48a4-b613-17b4ebd5cce7"), new DateTime(2025, 3, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khoa Ozil", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 6, false, true, null, "Rương 6", 90, null, null, null },
                    { new Guid("47e70b9d-ace8-42a8-b718-3119bc6359bf"), new Guid("ff62d2c7-f1e2-48a4-b613-17b4ebd5cce7"), new DateTime(2025, 3, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khoa Ozil", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 2, false, false, 20, "Rương 2", 90, null, null, null },
                    { new Guid("99133400-9c62-40f4-b491-fe0d8a87bd18"), new Guid("ff62d2c7-f1e2-48a4-b613-17b4ebd5cce7"), new DateTime(2025, 3, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khoa Ozil", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 4, false, false, 40, "Rương 4", 90, null, null, null },
                    { new Guid("d298d36c-18a8-4843-876c-dbcb1b06018c"), new Guid("ff62d2c7-f1e2-48a4-b613-17b4ebd5cce7"), new DateTime(2025, 3, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khoa Ozil", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 5, false, false, 50, "Rương 5", 90, null, null, null },
                    { new Guid("d9218ffd-70e6-4e05-a25e-37c585c6532e"), new Guid("ff62d2c7-f1e2-48a4-b613-17b4ebd5cce7"), new DateTime(2025, 3, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khoa Ozil", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 1, false, false, 10, "Rương 1", 90, null, null, null }
                });

            migrationBuilder.InsertData(
                table: "BlindBoxChestConfigs",
                columns: new[] { "Id", "BlindBoxChestId", "Coin", "ConfigType", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "ImagePath", "IsDeleted", "Percentage", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[,]
                {
                    { new Guid("054cd37b-e77c-46d8-8992-23c634f01470"), new Guid("99133400-9c62-40f4-b491-fe0d8a87bd18"), null, "GoodLuck", new DateTime(2025, 3, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khoa Ozil", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, 50, null, null, null },
                    { new Guid("0aa25d72-54f6-41e6-8960-8f9bbb94b88f"), new Guid("47e70b9d-ace8-42a8-b718-3119bc6359bf"), 45, "Coin", new DateTime(2025, 3, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khoa Ozil", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, 40, null, null, null },
                    { new Guid("16462388-8639-44fe-b1bf-db1859379fe4"), new Guid("374abb96-b10e-4fe7-82a7-79b20ec37548"), null, "Piece", new DateTime(2025, 3, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khoa Ozil", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, 10, null, null, null },
                    { new Guid("1aee9bd9-ded8-4dd6-ac4d-e790c4ee0083"), new Guid("374abb96-b10e-4fe7-82a7-79b20ec37548"), 45, "Coin", new DateTime(2025, 3, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khoa Ozil", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, 40, null, null, null },
                    { new Guid("50461e5e-a371-4251-b7de-4f29b222b431"), new Guid("d298d36c-18a8-4843-876c-dbcb1b06018c"), 45, "Coin", new DateTime(2025, 3, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khoa Ozil", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, 40, null, null, null },
                    { new Guid("5923d3f2-68a9-4d10-8fae-5aa1f83c99d3"), new Guid("99133400-9c62-40f4-b491-fe0d8a87bd18"), 45, "Coin", new DateTime(2025, 3, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khoa Ozil", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, 40, null, null, null },
                    { new Guid("5a5394be-9b8c-40f0-856b-55836a0fb23c"), new Guid("d9218ffd-70e6-4e05-a25e-37c585c6532e"), null, "GoodLuck", new DateTime(2025, 3, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khoa Ozil", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, 50, null, null, null },
                    { new Guid("5d13ece6-654f-4744-8600-ead3a9a31200"), new Guid("47e70b9d-ace8-42a8-b718-3119bc6359bf"), null, "Piece", new DateTime(2025, 3, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khoa Ozil", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, 10, null, null, null },
                    { new Guid("613f7065-18cf-4584-bbad-fea327286db0"), new Guid("374abb96-b10e-4fe7-82a7-79b20ec37548"), null, "GoodLuck", new DateTime(2025, 3, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khoa Ozil", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, 50, null, null, null },
                    { new Guid("6226db25-a2f5-4e4f-8e53-becf8a12f4b5"), new Guid("1632fece-f46e-4ce5-a374-13c74f61ae58"), null, "Piece", new DateTime(2025, 3, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khoa Ozil", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, 10, null, null, null },
                    { new Guid("732df173-39d0-435e-9bf0-ec10ea5ce4ed"), new Guid("1632fece-f46e-4ce5-a374-13c74f61ae58"), 45, "Coin", new DateTime(2025, 3, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khoa Ozil", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, 40, null, null, null },
                    { new Guid("85e75f6c-d9e8-4d66-97ed-3dcd395e6cd6"), new Guid("d298d36c-18a8-4843-876c-dbcb1b06018c"), null, "GoodLuck", new DateTime(2025, 3, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khoa Ozil", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, 50, null, null, null },
                    { new Guid("987a25cf-3cb2-4f7a-9fe2-1775a56424da"), new Guid("47e70b9d-ace8-42a8-b718-3119bc6359bf"), null, "GoodLuck", new DateTime(2025, 3, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khoa Ozil", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, 50, null, null, null },
                    { new Guid("c567813c-d7fc-4771-baf0-b4da940ceb48"), new Guid("99133400-9c62-40f4-b491-fe0d8a87bd18"), null, "Piece", new DateTime(2025, 3, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khoa Ozil", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, 10, null, null, null },
                    { new Guid("c617a036-c317-46f2-b716-d9ed0ab26398"), new Guid("1632fece-f46e-4ce5-a374-13c74f61ae58"), null, "GoodLuck", new DateTime(2025, 3, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khoa Ozil", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, 50, null, null, null },
                    { new Guid("d6e5a03e-2524-45c2-be20-5bbfd1b2bff3"), new Guid("d9218ffd-70e6-4e05-a25e-37c585c6532e"), null, "Piece", new DateTime(2025, 3, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khoa Ozil", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, 10, null, null, null },
                    { new Guid("eb223acc-d4bb-45f2-a2cf-1d7464b596bb"), new Guid("d298d36c-18a8-4843-876c-dbcb1b06018c"), null, "Piece", new DateTime(2025, 3, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khoa Ozil", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "", false, 10, null, null, null },
                    { new Guid("eb5a0855-343c-4b53-aa4c-46915eeac820"), new Guid("d9218ffd-70e6-4e05-a25e-37c585c6532e"), 45, "Coin", new DateTime(2025, 3, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "Khoa Ozil", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, 40, null, null, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_BlindBoxChestConfigs_BlindBoxChestId",
                table: "BlindBoxChestConfigs",
                column: "BlindBoxChestId");

            migrationBuilder.CreateIndex(
                name: "IX_BlindBoxChests_BlindBoxId",
                table: "BlindBoxChests",
                column: "BlindBoxId");

            migrationBuilder.CreateIndex(
                name: "IX_BlindBoxHistories_BlindBoxChestConfigId",
                table: "BlindBoxHistories",
                column: "BlindBoxChestConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_BlindBoxUsers_BlindBoxId",
                table: "BlindBoxUsers",
                column: "BlindBoxId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BlindBoxHistories");

            migrationBuilder.DropTable(
                name: "BlindBoxUsers");

            migrationBuilder.DropTable(
                name: "BlindBoxChestConfigs");

            migrationBuilder.DropTable(
                name: "BlindBoxChests");

            migrationBuilder.DropTable(
                name: "BlindBoxes");
        }
    }
}
