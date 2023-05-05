using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Identity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateCSO : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CourseLevelsStr",
                table: "Teachers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CourseTypesStr",
                table: "Teachers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "CSOs",
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
                    CourseTypesStr = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CourseLevelsStr = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PassportPath = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    UniversityDegreePath = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CertificationPath = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    PoliceClearancePath = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    HumanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CSOs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CSOs_Humans_HumanId",
                        column: x => x.HumanId,
                        principalTable: "Humans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CSOs_HumanId",
                table: "CSOs",
                column: "HumanId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CSOs");

            migrationBuilder.DropColumn(
                name: "CourseLevelsStr",
                table: "Teachers");

            migrationBuilder.DropColumn(
                name: "CourseTypesStr",
                table: "Teachers");
        }
    }
}
