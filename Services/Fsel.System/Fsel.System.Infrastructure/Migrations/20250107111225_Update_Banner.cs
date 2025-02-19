using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.System.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_Banner : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApplicableUserGroupsStr",
                table: "BannerScopes");

            migrationBuilder.DropColumn(
                name: "CompetitionEventIdsStr",
                table: "BannerScopes");

            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "Banners");

            migrationBuilder.DropColumn(
                name: "RouteScreen",
                table: "Banners");

            migrationBuilder.DropColumn(
                name: "Url",
                table: "Banners");

            migrationBuilder.AddColumn<long>(
                name: "TimeSlideShow",
                table: "BannerSettings",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AlterColumn<string>(
                name: "CourseLevel",
                table: "BannerScopes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<int>(
                name: "ApplicableUser",
                table: "BannerScopes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "CompetitionEventId",
                table: "BannerScopes",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "MixPanel",
                table: "Banners",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "BannerImages",
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
                    BannerImageType = table.Column<int>(type: "int", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(1500)", maxLength: 1500, nullable: false),
                    RouteScreen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Url = table.Column<string>(type: "nvarchar(1500)", maxLength: 1500, nullable: true),
                    BannerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BannerImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BannerImages_Banners_BannerId",
                        column: x => x.BannerId,
                        principalTable: "Banners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BannerImages_BannerId",
                table: "BannerImages",
                column: "BannerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BannerImages");

            migrationBuilder.DropColumn(
                name: "TimeSlideShow",
                table: "BannerSettings");

            migrationBuilder.DropColumn(
                name: "ApplicableUser",
                table: "BannerScopes");

            migrationBuilder.DropColumn(
                name: "CompetitionEventId",
                table: "BannerScopes");

            migrationBuilder.DropColumn(
                name: "MixPanel",
                table: "Banners");

            migrationBuilder.AlterColumn<string>(
                name: "CourseLevel",
                table: "BannerScopes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApplicableUserGroupsStr",
                table: "BannerScopes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompetitionEventIdsStr",
                table: "BannerScopes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "Banners",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RouteScreen",
                table: "Banners",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Url",
                table: "Banners",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);
        }
    }
}
