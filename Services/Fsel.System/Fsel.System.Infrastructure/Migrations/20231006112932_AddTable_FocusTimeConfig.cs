using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Fsel.System.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTable_FocusTimeConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FocusTimeConfigs",
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
                    TargetTime = table.Column<double>(type: "float", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Token = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FocusTimeConfigs", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "FocusTimeConfigs",
                columns: new[] { "Id", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "Description", "IsDeleted", "TargetTime", "Token", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[,]
                {
                    { new Guid("0a7b57f3-c964-4f1b-8986-df1579c5d08b"), new DateTime(2023, 10, 6, 18, 29, 32, 470, DateTimeKind.Local).AddTicks(669), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Bắt đầu nhẹ nhàng", false, 30.0, 1, null, null, null },
                    { new Guid("124f4341-4c87-4e5f-ba4d-2481d8d36737"), new DateTime(2023, 10, 6, 18, 29, 32, 470, DateTimeKind.Local).AddTicks(1038), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Tập trung hơn nữa nào", false, 90.0, 6, null, null, null },
                    { new Guid("82061293-c9d0-4598-99f1-8dfd8162b999"), new DateTime(2023, 10, 6, 18, 29, 32, 470, DateTimeKind.Local).AddTicks(1054), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Chăm chỉ phết", false, 120.0, 12, null, null, null },
                    { new Guid("9b4fa7b6-1af4-458b-82d9-621c1a88654a"), new DateTime(2023, 10, 6, 18, 29, 32, 470, DateTimeKind.Local).AddTicks(1014), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Hình thành thói quen chưa?", false, 60.0, 3, null, null, null },
                    { new Guid("fd7e66d3-a29b-4da7-bb29-2283536d836a"), new DateTime(2023, 10, 6, 18, 29, 32, 470, DateTimeKind.Local).AddTicks(1069), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Thách đấu FSEL", false, 180.0, 24, null, null, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FocusTimeConfigs");
        }
    }
}
