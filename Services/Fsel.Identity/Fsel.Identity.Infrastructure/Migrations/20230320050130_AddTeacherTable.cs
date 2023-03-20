using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Fsel.Identity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTeacherTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "1e63ac94-5e95-4fe1-a534-548bb972eabf");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "3a715a55-6a0a-47a9-b810-f8f1108514b4");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "755c38e5-3f70-4458-83c6-b85f486aeeb1");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "97f5de28-9a99-415d-8932-f2918d36e6fa");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "bab858f3-f52a-47cd-a9fc-af5d02bebd79");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "d206fb0d-86af-4c16-9fc4-502efad2dbf2");

            migrationBuilder.CreateTable(
                name: "Teachers",
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
                    PassportPath = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    UniversityDegreePath = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CertificationPath = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    PoliceClearancePath = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    HumanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teachers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Teachers_Humans_HumanId",
                        column: x => x.HumanId,
                        principalTable: "Humans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Discriminator", "Discription", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "069ae2a9-2729-4905-a8fa-c6c9f9172d1d", null, "Role", null, "Student", "Student" },
                    { "1eeba88e-ed0c-48c4-8d7e-a17c5a2cc3fe", null, "Role", null, "Teacher", "Teacher" },
                    { "24ce207d-8732-4a32-83ef-c5f05805f124", null, "Role", null, "Parent", "Parent" },
                    { "69976022-5dbb-4292-bab6-e94b6701061e", null, "Role", null, "Admin", "Admin" },
                    { "8d89a29a-b40a-4045-b0ed-679d7a5ff990", null, "Role", null, "CSO", "CSO" },
                    { "c8c28631-0bc0-4166-b062-4aff0d38e70c", null, "Role", null, "MasterAdmin", "MasterAdmin" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Teachers_HumanId",
                table: "Teachers",
                column: "HumanId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Teachers");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "069ae2a9-2729-4905-a8fa-c6c9f9172d1d");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "1eeba88e-ed0c-48c4-8d7e-a17c5a2cc3fe");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "24ce207d-8732-4a32-83ef-c5f05805f124");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "69976022-5dbb-4292-bab6-e94b6701061e");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "8d89a29a-b40a-4045-b0ed-679d7a5ff990");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "c8c28631-0bc0-4166-b062-4aff0d38e70c");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Discriminator", "Discription", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "1e63ac94-5e95-4fe1-a534-548bb972eabf", null, "Role", null, "Admin", "Admin" },
                    { "3a715a55-6a0a-47a9-b810-f8f1108514b4", null, "Role", null, "MasterAdmin", "MasterAdmin" },
                    { "755c38e5-3f70-4458-83c6-b85f486aeeb1", null, "Role", null, "CSO", "Parent" },
                    { "97f5de28-9a99-415d-8932-f2918d36e6fa", null, "Role", null, "CSO", "Teacher" },
                    { "bab858f3-f52a-47cd-a9fc-af5d02bebd79", null, "Role", null, "CSO", "CSO" },
                    { "d206fb0d-86af-4c16-9fc4-502efad2dbf2", null, "Role", null, "CSO", "Student" }
                });
        }
    }
}
