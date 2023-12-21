using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.System.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTable_ApprovalLog_Column_ApprovalType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ApprovalTimeType",
                table: "ApprovalTimeConfigs",
                newName: "ApprovalType");

            migrationBuilder.CreateTable(
                name: "ApprovalLog",
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
                    ExpiredDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ObjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserIdsStr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApprovalTimeConfigId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApprovalLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApprovalLog_ApprovalTimeConfigs_ApprovalTimeConfigId",
                        column: x => x.ApprovalTimeConfigId,
                        principalTable: "ApprovalTimeConfigs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "ApprovalTimeConfigs",
                keyColumn: "Id",
                keyValue: new Guid("80b1eee1-ddb1-4599-8008-959c5e3f6bb2"),
                column: "ApprovalType",
                value: "DiscussionBoard");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalLog_ApprovalTimeConfigId",
                table: "ApprovalLog",
                column: "ApprovalTimeConfigId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApprovalLog");

            migrationBuilder.RenameColumn(
                name: "ApprovalType",
                table: "ApprovalTimeConfigs",
                newName: "ApprovalTimeType");

            migrationBuilder.UpdateData(
                table: "ApprovalTimeConfigs",
                keyColumn: "Id",
                keyValue: new Guid("80b1eee1-ddb1-4599-8008-959c5e3f6bb2"),
                column: "ApprovalTimeType",
                value: "DiscussionBoardFlag");
        }
    }
}
