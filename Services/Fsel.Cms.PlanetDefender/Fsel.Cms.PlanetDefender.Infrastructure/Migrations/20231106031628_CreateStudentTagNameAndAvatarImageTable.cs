using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Fsel.Cms.PlanetDefender.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateStudentTagNameAndAvatarImageTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Level",
                table: "StudentGameInfos",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<Guid>(
                name: "AvatarImageId",
                table: "StudentGameInfos",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "CourseLevel",
                table: "StudentGameInfos",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "StudentTagNameId",
                table: "StudentGameInfos",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TagNameId",
                table: "StudentGameInfos",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "SpaceShipId",
                table: "SpaceShips",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AvatarImages",
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
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Level = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AvatarImages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GameHistories",
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
                    RoundNumber = table.Column<int>(type: "int", nullable: false),
                    Score = table.Column<long>(type: "bigint", nullable: false),
                    StudentGameInfoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SpaceShipId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NumberOfToken = table.Column<int>(type: "int", nullable: false),
                    ImpactNumber = table.Column<int>(type: "int", nullable: false),
                    DestroyNumber = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GameHistories_SpaceShips_SpaceShipId",
                        column: x => x.SpaceShipId,
                        principalTable: "SpaceShips",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GameHistories_StudentGameInfos_StudentGameInfoId",
                        column: x => x.StudentGameInfoId,
                        principalTable: "StudentGameInfos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StudentSpaceShips",
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
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Level = table.Column<int>(type: "int", nullable: false),
                    StudentGameInfoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SpaceShipId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentSpaceShips", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentSpaceShips_SpaceShips_SpaceShipId",
                        column: x => x.SpaceShipId,
                        principalTable: "SpaceShips",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentSpaceShips_StudentGameInfos_StudentGameInfoId",
                        column: x => x.StudentGameInfoId,
                        principalTable: "StudentGameInfos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StudentTagNames",
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
                    Level = table.Column<int>(type: "int", nullable: false),
                    TagName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    MaxLevelSpaceShipId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SpaceShipId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentTagNames", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentTagNames_SpaceShips_SpaceShipId",
                        column: x => x.SpaceShipId,
                        principalTable: "SpaceShips",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "AvatarImages",
                columns: new[] { "Id", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "FilePath", "IsDeleted", "Level", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[] { new Guid("0e614a70-18b3-4fa1-9fff-632f1af667cd"), new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "https://fsel.s3-hn-2.cloud.cmctelecom.vn/videos/vetranhmeohoathinhdethuong_1699239452.jpg", false, 1, null, null, null });

            migrationBuilder.UpdateData(
                table: "SpaceShips",
                keyColumn: "Id",
                keyValue: new Guid("46be8251-f95a-4e1b-b451-2a3fe2b4a5bc"),
                column: "SpaceShipId",
                value: null);

            migrationBuilder.InsertData(
                table: "StudentTagNames",
                columns: new[] { "Id", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "IsDeleted", "Level", "MaxLevelSpaceShipId", "SpaceShipId", "TagName", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[,]
                {
                    { new Guid("402dec4e-39c9-4235-8c7d-b18eb2092305"), new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, 40, null, null, "General", null, null, null },
                    { new Guid("88bf6e6d-d9d6-4d52-bf9d-52f252f16404"), new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, 10, null, null, "Major", null, null, null },
                    { new Guid("9f860757-1a3e-4a05-aaf5-2dc1797e23bf"), new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, 1, null, null, "Captain", null, null, null },
                    { new Guid("b38637d8-129d-4d50-bdf5-a3304684740d"), new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, 30, null, null, "Brigadier", null, null, null },
                    { new Guid("e2ef7a2e-bc23-4103-8e2b-23cfdca5264e"), new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, 50, null, null, "Supreme Leader", null, null, null },
                    { new Guid("edc52c09-c407-4371-9201-8f09351c1f58"), new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, 20, null, null, "Colonel", null, null, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_StudentGameInfos_AvatarImageId",
                table: "StudentGameInfos",
                column: "AvatarImageId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentGameInfos_StudentTagNameId",
                table: "StudentGameInfos",
                column: "StudentTagNameId");

            migrationBuilder.CreateIndex(
                name: "IX_SpaceShips_SpaceShipId",
                table: "SpaceShips",
                column: "SpaceShipId");

            migrationBuilder.CreateIndex(
                name: "IX_GameHistories_SpaceShipId",
                table: "GameHistories",
                column: "SpaceShipId");

            migrationBuilder.CreateIndex(
                name: "IX_GameHistories_StudentGameInfoId",
                table: "GameHistories",
                column: "StudentGameInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentSpaceShips_SpaceShipId",
                table: "StudentSpaceShips",
                column: "SpaceShipId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentSpaceShips_StudentGameInfoId",
                table: "StudentSpaceShips",
                column: "StudentGameInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentTagNames_SpaceShipId",
                table: "StudentTagNames",
                column: "SpaceShipId");

            migrationBuilder.AddForeignKey(
                name: "FK_SpaceShips_SpaceShips_SpaceShipId",
                table: "SpaceShips",
                column: "SpaceShipId",
                principalTable: "SpaceShips",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentGameInfos_AvatarImages_AvatarImageId",
                table: "StudentGameInfos",
                column: "AvatarImageId",
                principalTable: "AvatarImages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentGameInfos_StudentTagNames_StudentTagNameId",
                table: "StudentGameInfos",
                column: "StudentTagNameId",
                principalTable: "StudentTagNames",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SpaceShips_SpaceShips_SpaceShipId",
                table: "SpaceShips");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentGameInfos_AvatarImages_AvatarImageId",
                table: "StudentGameInfos");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentGameInfos_StudentTagNames_StudentTagNameId",
                table: "StudentGameInfos");

            migrationBuilder.DropTable(
                name: "AvatarImages");

            migrationBuilder.DropTable(
                name: "GameHistories");

            migrationBuilder.DropTable(
                name: "StudentSpaceShips");

            migrationBuilder.DropTable(
                name: "StudentTagNames");

            migrationBuilder.DropIndex(
                name: "IX_StudentGameInfos_AvatarImageId",
                table: "StudentGameInfos");

            migrationBuilder.DropIndex(
                name: "IX_StudentGameInfos_StudentTagNameId",
                table: "StudentGameInfos");

            migrationBuilder.DropIndex(
                name: "IX_SpaceShips_SpaceShipId",
                table: "SpaceShips");

            migrationBuilder.DropColumn(
                name: "AvatarImageId",
                table: "StudentGameInfos");

            migrationBuilder.DropColumn(
                name: "CourseLevel",
                table: "StudentGameInfos");

            migrationBuilder.DropColumn(
                name: "StudentTagNameId",
                table: "StudentGameInfos");

            migrationBuilder.DropColumn(
                name: "TagNameId",
                table: "StudentGameInfos");

            migrationBuilder.DropColumn(
                name: "SpaceShipId",
                table: "SpaceShips");

            migrationBuilder.AlterColumn<string>(
                name: "Level",
                table: "StudentGameInfos",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
