using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Fsel.Cms.PlanetDefender.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateZmatterTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                table: "ZMatters",
                columns: new[] { "Id", "Code", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "Description", "FilePath", "IsActive", "IsDeleted", "Name", "UpdatedDate", "UpdatedFullName", "UpdatedUserId", "Usage" },
                values: new object[,]
                {
                    { new Guid("0cf0c6b3-1312-473d-bd2a-15dfa88d6052"), "PU001", new DateTime(2023, 10, 12, 17, 39, 10, 846, DateTimeKind.Local).AddTicks(4186), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "This power...It's strange", null, false, false, "Power Up", null, null, null, "Restores 20% rage" },
                    { new Guid("12b08f82-9b0b-4a7a-92d2-10e35b9fea4e"), "HI001", new DateTime(2023, 10, 12, 17, 39, 10, 846, DateTimeKind.Local).AddTicks(4193), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Whose card is this?", null, false, false, "Hacker ID", null, null, null, "Only use when answering a question, immediately display the answer and answer" },
                    { new Guid("187c1ce2-23ad-4acc-ba89-649a437c1099"), "WB001", new DateTime(2023, 10, 12, 17, 39, 10, 846, DateTimeKind.Local).AddTicks(4139), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "A sword without a costume? Oh no, look, there's a sharp wind around it!", null, false, false, "Wind Blade", null, null, null, "Push 1 meteor, reset meteor duration to maximum" },
                    { new Guid("192b995e-9cfa-45ee-9f6f-f6fff5803059"), "GB001", new DateTime(2023, 10, 12, 17, 39, 10, 846, DateTimeKind.Local).AddTicks(4190), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "What kind of bomb sticks like glue?", null, false, false, "Gum Bomp", null, null, null, "Place the bomb in one location, when the meteorite sticks it will explode, causing the meteorite to stand still for 2 seconds" },
                    { new Guid("54c17216-a173-4ed6-a6e8-ff7062494705"), "SP001", new DateTime(2023, 10, 12, 17, 39, 10, 846, DateTimeKind.Local).AddTicks(4183), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "The rescue ship is here!", null, false, false, "Supply Kit", null, null, null, "Use immediately restores 10% of maximum health (Full will restore shield)" },
                    { new Guid("9faaead9-d4de-4aa8-8523-5432fa7f313f"), "SW001", new DateTime(2023, 10, 12, 17, 39, 10, 846, DateTimeKind.Local).AddTicks(4177), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "What's the use of a broken watch?", null, false, false, "Stop Watch", null, null, null, "Freeze time within X seconds" },
                    { new Guid("baecdd2b-39c3-41a5-8240-1a48e37b4f33"), "SH001", new DateTime(2023, 10, 12, 17, 39, 10, 846, DateTimeKind.Local).AddTicks(4180), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "This shield is so beautiful! Wish it was here forever", null, false, false, "Shield", null, null, null, "Quantum shield, helps the spacecraft block 1 damage" },
                    { new Guid("fdc945ba-248e-4db2-96cb-c7d55a7de8c8"), "MG001", new DateTime(2023, 10, 12, 17, 39, 10, 846, DateTimeKind.Local).AddTicks(4188), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "With the force of fate, these meteorites need a little help", null, false, false, "Magnetic", null, null, null, "Creates a link between 2 meteorites, answering 1 meteorite correctly will destroy both meteorites (If 1 meteorite answers incorrectly, you can still answer the remaining question to destroy both)" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ZMatters");
        }
    }
}
