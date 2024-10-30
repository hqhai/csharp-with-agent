using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.System.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_Table_Banner : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Banners",
                newName: "Content");

            migrationBuilder.AlterColumn<string>(
                name: "Url",
                table: "Banners",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000);

            migrationBuilder.AddColumn<string>(
                name: "BannerFrequency",
                table: "Banners",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "DisplayEndDate",
                table: "Banners",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DisplayEndTime",
                table: "Banners",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DisplayStartDate",
                table: "Banners",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DisplayStartTime",
                table: "Banners",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Status",
                table: "Banners",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "BannerScopes",
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
                    IsPriority = table.Column<bool>(type: "bit", nullable: false),
                    TargetUserStr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CourseLevel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ApplicableUserGroupsStr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompetitionEventIdsStr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BannerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BannerScopes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BannerScopes_Banners_BannerId",
                        column: x => x.BannerId,
                        principalTable: "Banners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BannerSettings",
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
                    MaximumPerDay = table.Column<int>(type: "int", nullable: false),
                    DisplayIntervalTime = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BannerSettings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BannerScopes_BannerId",
                table: "BannerScopes",
                column: "BannerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BannerScopes");

            migrationBuilder.DropTable(
                name: "BannerSettings");

            migrationBuilder.DropColumn(
                name: "BannerFrequency",
                table: "Banners");

            migrationBuilder.DropColumn(
                name: "DisplayEndDate",
                table: "Banners");

            migrationBuilder.DropColumn(
                name: "DisplayEndTime",
                table: "Banners");

            migrationBuilder.DropColumn(
                name: "DisplayStartDate",
                table: "Banners");

            migrationBuilder.DropColumn(
                name: "DisplayStartTime",
                table: "Banners");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Banners");

            migrationBuilder.RenameColumn(
                name: "Content",
                table: "Banners",
                newName: "Description");

            migrationBuilder.AlterColumn<string>(
                name: "Url",
                table: "Banners",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);
        }
    }
}
