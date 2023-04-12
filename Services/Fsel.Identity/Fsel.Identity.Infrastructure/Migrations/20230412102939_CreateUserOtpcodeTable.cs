using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Identity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateUserOtpcodeTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Teachers_HumanId",
                table: "Teachers");

            migrationBuilder.DropIndex(
                name: "IX_Students_HumanId",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Parents_HumanId",
                table: "Parents");

            migrationBuilder.DropIndex(
                name: "IX_Humans_UserId",
                table: "Humans");

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Humans",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "UserOtpCodes",
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
                    OTPCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExpiredTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserOtpCodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserOtpCodes_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Teachers_HumanId",
                table: "Teachers",
                column: "HumanId");

            migrationBuilder.CreateIndex(
                name: "IX_Students_HumanId",
                table: "Students",
                column: "HumanId");

            migrationBuilder.CreateIndex(
                name: "IX_Parents_HumanId",
                table: "Parents",
                column: "HumanId");

            migrationBuilder.CreateIndex(
                name: "IX_Humans_UserId",
                table: "Humans",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserOtpCodes_UserId",
                table: "UserOtpCodes",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserOtpCodes");

            migrationBuilder.DropIndex(
                name: "IX_Teachers_HumanId",
                table: "Teachers");

            migrationBuilder.DropIndex(
                name: "IX_Students_HumanId",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Parents_HumanId",
                table: "Parents");

            migrationBuilder.DropIndex(
                name: "IX_Humans_UserId",
                table: "Humans");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "Humans");

            migrationBuilder.CreateIndex(
                name: "IX_Teachers_HumanId",
                table: "Teachers",
                column: "HumanId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Students_HumanId",
                table: "Students",
                column: "HumanId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Parents_HumanId",
                table: "Parents",
                column: "HumanId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Humans_UserId",
                table: "Humans",
                column: "UserId",
                unique: true,
                filter: "[UserId] IS NOT NULL");
        }
    }
}
