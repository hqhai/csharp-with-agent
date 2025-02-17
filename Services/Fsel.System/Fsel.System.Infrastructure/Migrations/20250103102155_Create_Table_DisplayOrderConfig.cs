using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Fsel.System.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Create_Table_DisplayOrderConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DisplayOrderConfig",
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
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Status = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DisplayOrderConfig", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "DisplayOrderConfig",
                columns: new[] { "Id", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "DisplayOrder", "IsDeleted", "Name", "Status", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[,]
                {
                    { new Guid("08db065d-689f-42bc-95bd-5181fb4d7024"), new DateTime(2025, 1, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 6, false, "TwentyFiveMinutesBreak", true, null, null, null },
                    { new Guid("29f6b274-6edd-427b-b1be-51a8ba5b23cd"), new DateTime(2025, 1, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 7, false, "Beginner", true, null, null, null },
                    { new Guid("5f787b73-36cf-4db9-8378-74ffacef6df1"), new DateTime(2025, 1, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 3, false, "GoodMorning", true, null, null, null },
                    { new Guid("858c0ac1-41a2-461b-9252-32f56914caf0"), new DateTime(2025, 1, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 5, false, "StreakTimeCode", true, null, null, null },
                    { new Guid("ac124367-9b60-46dd-964f-aa85efc91ada"), new DateTime(2025, 1, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 1, false, "Popup", true, null, null, null },
                    { new Guid("dac69796-079e-4090-9b8f-e786856d4a20"), new DateTime(2025, 1, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 4, false, "FselCoin", true, null, null, null },
                    { new Guid("f9ee8b6b-d3b5-4183-9b4f-62c373c80020"), new DateTime(2025, 1, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 2, false, "Heading", true, null, null, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DisplayOrderConfig");
        }
    }
}
