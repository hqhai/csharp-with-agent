using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Fsel.Cms.PlanetDefender.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Create_DB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GameplayRuleConfigs",
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
                    StartRoundNumber = table.Column<int>(type: "int", nullable: false),
                    EndRoundNumber = table.Column<int>(type: "int", nullable: false),
                    CurrentUnit = table.Column<int>(type: "int", nullable: false),
                    CurrentUnitOutside = table.Column<int>(type: "int", nullable: false),
                    PreviousUnit = table.Column<int>(type: "int", nullable: false),
                    PreviousUnitOutside = table.Column<int>(type: "int", nullable: false),
                    NumberQuestionPerGame = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameplayRuleConfigs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GameplayTimeConfigs",
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
                    RoundNumber = table.Column<int>(type: "int", nullable: false),
                    GameVocabPDType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Time = table.Column<double>(type: "float", nullable: false),
                    Percent = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameplayTimeConfigs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SpaceShips",
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
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpaceShips", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StudentGameInfos",
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
                    Level = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NickName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Gender = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentGameInfos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WheelOfBuffs",
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
                    Type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WheelOfBuffs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ZMatters",
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
                    Name = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Usage = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZMatters", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "GameplayRuleConfigs",
                columns: new[] { "Id", "CreatedDate", "CreatedFullName", "CreatedUserId", "CurrentUnit", "CurrentUnitOutside", "DeletedDate", "DeletedFullName", "DeletedUserId", "EndRoundNumber", "IsDeleted", "NumberQuestionPerGame", "PreviousUnit", "PreviousUnitOutside", "StartRoundNumber", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[,]
                {
                    { new Guid("a3561911-db7f-4942-a5bb-8a2615e4dca8"), new DateTime(2023, 10, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), "admin", new Guid("00000000-0000-0000-0000-000000000000"), 5, 5, null, null, null, 10000, false, 20, 5, 5, 10, null, null, null },
                    { new Guid("ad566fae-9d00-48b7-a76c-e8fe84a9125f"), new DateTime(2023, 10, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), "admin", new Guid("00000000-0000-0000-0000-000000000000"), 2, 4, null, null, null, 9, false, 10, 2, 2, 1, null, null, null }
                });

            migrationBuilder.InsertData(
                table: "SpaceShips",
                columns: new[] { "Id", "Code", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "IsDefault", "IsDeleted", "Name", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[] { new Guid("46be8251-f95a-4e1b-b451-2a3fe2b4a5bc"), "AG222", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, true, false, "Atlantic", null, null, null });

            migrationBuilder.InsertData(
                table: "WheelOfBuffs",
                columns: new[] { "Id", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "IsActive", "IsDeleted", "Type", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[,]
                {
                    { new Guid("352a53f5-1314-47f3-a953-16f3b4f508df"), new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, true, false, "TenFSELcoin", null, null, null },
                    { new Guid("376b7f0c-e8e6-4244-b2f7-149093ec7a5f"), new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, true, false, "HealBarShield", null, null, null },
                    { new Guid("41f39ea2-5a2b-41c1-86cc-d9baee68c477"), new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, true, false, "TwentyFSELcoin", null, null, null },
                    { new Guid("4e36a6d2-6e01-4c3f-8ac2-142e2db9bf67"), new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, true, false, "X", null, null, null },
                    { new Guid("54a848c5-8d29-4698-915c-6e010048568d"), new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, true, false, "ReceiveZBuff", null, null, null },
                    { new Guid("70b3f720-ae44-4f06-8bc3-aa1b05404ffe"), new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, true, false, "TenFSELcoin", null, null, null },
                    { new Guid("813899b1-c223-4bfc-a054-e061c98987be"), new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, true, false, "TenFSELcoin", null, null, null },
                    { new Guid("8e4f6f33-bb51-47fc-b7ac-74a749c94fcd"), new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, true, false, "HealBarShield", null, null, null },
                    { new Guid("9accf434-fd6a-4073-b0cd-e84e5b042020"), new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, true, false, "ReceiveZBuff", null, null, null },
                    { new Guid("cedea4e9-f2f2-4005-9fa0-221095f51d1d"), new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, true, false, "FiftyFSELcoin", null, null, null }
                });

            migrationBuilder.InsertData(
                table: "ZMatters",
                columns: new[] { "Id", "Code", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "Description", "FilePath", "IsActive", "IsDeleted", "Name", "UpdatedDate", "UpdatedFullName", "UpdatedUserId", "Usage" },
                values: new object[,]
                {
                    { new Guid("0cf0c6b3-1312-473d-bd2a-15dfa88d6052"), "PU001", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "This power...It's strange", null, false, false, "Power Up", null, null, null, "Restores 20% rage" },
                    { new Guid("12b08f82-9b0b-4a7a-92d2-10e35b9fea4e"), "HI001", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Whose card is this?", null, false, false, "Hacker ID", null, null, null, "Only use when answering a question, immediately display the answer and answer" },
                    { new Guid("187c1ce2-23ad-4acc-ba89-649a437c1099"), "WB001", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "A sword without a costume? Oh no, look, there's a sharp wind around it!", null, false, false, "Wind Blade", null, null, null, "Push 1 meteor, reset meteor duration to maximum" },
                    { new Guid("192b995e-9cfa-45ee-9f6f-f6fff5803059"), "GB001", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "What kind of bomb sticks like glue?", null, false, false, "Gum Bomp", null, null, null, "Place the bomb in one location, when the meteorite sticks it will explode, causing the meteorite to stand still for 2 seconds" },
                    { new Guid("54c17216-a173-4ed6-a6e8-ff7062494705"), "SP001", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "The rescue ship is here!", null, false, false, "Supply Kit", null, null, null, "Use immediately restores 10% of maximum health (Full will restore shield)" },
                    { new Guid("9faaead9-d4de-4aa8-8523-5432fa7f313f"), "SW001", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "What's the use of a broken watch?", null, false, false, "Stop Watch", null, null, null, "Freeze time within X seconds" },
                    { new Guid("baecdd2b-39c3-41a5-8240-1a48e37b4f33"), "SH001", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "This shield is so beautiful! Wish it was here forever", null, false, false, "Shield", null, null, null, "Quantum shield, helps the spacecraft block 1 damage" },
                    { new Guid("fdc945ba-248e-4db2-96cb-c7d55a7de8c8"), "MG001", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "With the force of fate, these meteorites need a little help", null, false, false, "Magnetic", null, null, null, "Creates a link between 2 meteorites, answering 1 meteorite correctly will destroy both meteorites (If 1 meteorite answers incorrectly, you can still answer the remaining question to destroy both)" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GameplayRuleConfigs");

            migrationBuilder.DropTable(
                name: "GameplayTimeConfigs");

            migrationBuilder.DropTable(
                name: "SpaceShips");

            migrationBuilder.DropTable(
                name: "StudentGameInfos");

            migrationBuilder.DropTable(
                name: "WheelOfBuffs");

            migrationBuilder.DropTable(
                name: "ZMatters");
        }
    }
}
