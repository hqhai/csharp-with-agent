using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_Table_ClassForumDetailResultHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ClassForumDetailResultHistoryId",
                table: "ClassForumResultFiles",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ClassForumDetailResultHistories",
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
                    Content = table.Column<string>(type: "nvarchar(max)", maxLength: 10000, nullable: true),
                    WordContent = table.Column<string>(type: "nvarchar(max)", maxLength: 10000, nullable: true),
                    ClassForumDetailResultId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassForumDetailResultHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClassForumDetailResultHistories_ClassForumDetailResults_ClassForumDetailResultId",
                        column: x => x.ClassForumDetailResultId,
                        principalTable: "ClassForumDetailResults",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClassForumResultFiles_ClassForumDetailResultHistoryId",
                table: "ClassForumResultFiles",
                column: "ClassForumDetailResultHistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassForumDetailResultHistories_ClassForumDetailResultId",
                table: "ClassForumDetailResultHistories",
                column: "ClassForumDetailResultId");

            migrationBuilder.AddForeignKey(
                name: "FK_ClassForumResultFiles_ClassForumDetailResultHistories_ClassForumDetailResultHistoryId",
                table: "ClassForumResultFiles",
                column: "ClassForumDetailResultHistoryId",
                principalTable: "ClassForumDetailResultHistories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClassForumResultFiles_ClassForumDetailResultHistories_ClassForumDetailResultHistoryId",
                table: "ClassForumResultFiles");

            migrationBuilder.DropTable(
                name: "ClassForumDetailResultHistories");

            migrationBuilder.DropIndex(
                name: "IX_ClassForumResultFiles_ClassForumDetailResultHistoryId",
                table: "ClassForumResultFiles");

            migrationBuilder.DropColumn(
                name: "ClassForumDetailResultHistoryId",
                table: "ClassForumResultFiles");
        }
    }
}
