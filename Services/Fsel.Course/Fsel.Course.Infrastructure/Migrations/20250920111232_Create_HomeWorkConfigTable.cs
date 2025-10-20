using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Course.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Create_HomeWorkConfigTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HomeWorkConfigs",
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
                    NumberRetry = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HomeWorkId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CurriculumId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CurriculumConfigId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HomeWorkConfigs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HomeWorkConfigs_CurriculumConfigs_CurriculumConfigId",
                        column: x => x.CurriculumConfigId,
                        principalTable: "CurriculumConfigs",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_HomeWorkConfigs_HomeWorks_HomeWorkId",
                        column: x => x.HomeWorkId,
                        principalTable: "HomeWorks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HomeWorkConfigs_CurriculumConfigId",
                table: "HomeWorkConfigs",
                column: "CurriculumConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_HomeWorkConfigs_HomeWorkId",
                table: "HomeWorkConfigs",
                column: "HomeWorkId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HomeWorkConfigs");
        }
    }
}
