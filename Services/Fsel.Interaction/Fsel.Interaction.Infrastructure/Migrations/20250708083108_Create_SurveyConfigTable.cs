using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Interaction.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Create_SurveyConfigTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsRequired",
                table: "SurveyQuestions",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SurveyConfigId",
                table: "SurveyQuestions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SurveyConfigs",
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
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Tokens = table.Column<int>(type: "int", nullable: false),
                    ApplicableProgram = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CompetitionEventIdsStr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApplicableSubjectsStr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProgressRequirementsStr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SurveyConfigs", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("03d12e43-250b-49b7-bd08-b12135e47723"),
                columns: new[] { "IsRequired", "SurveyConfigId" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("06b1a0df-eaef-4be9-a9da-9c2633e3a12a"),
                columns: new[] { "IsRequired", "SurveyConfigId" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("0aafccb7-e700-4b30-866f-0b667217fa47"),
                columns: new[] { "IsRequired", "SurveyConfigId" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("0f9df809-9ae9-4e8c-891d-ec645483f290"),
                columns: new[] { "IsRequired", "SurveyConfigId" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("14787cbf-cc43-4453-a148-6d11a683f311"),
                columns: new[] { "IsRequired", "SurveyConfigId" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("1fde61af-e1e0-4027-ad21-1176ad212119"),
                columns: new[] { "IsRequired", "SurveyConfigId" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("20675350-89f2-47e3-8e64-b04f2da6290b"),
                columns: new[] { "IsRequired", "SurveyConfigId" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("238cae89-e1d8-408a-bb3c-1a0b27cd0edb"),
                columns: new[] { "IsRequired", "SurveyConfigId" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("2be9a620-143d-41f6-815b-2038c21a7b23"),
                columns: new[] { "IsRequired", "SurveyConfigId" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("35a71ae7-49c1-4878-a1db-edcd2834f1cd"),
                columns: new[] { "IsRequired", "SurveyConfigId" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("482c87c7-b555-4de2-a418-f990960e0ee6"),
                columns: new[] { "IsRequired", "SurveyConfigId" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("492d8bb9-cdbe-42e7-aa16-35a1915c3621"),
                columns: new[] { "IsRequired", "SurveyConfigId" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("52548bc5-3536-478c-978b-05f98815bf31"),
                columns: new[] { "IsRequired", "SurveyConfigId" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("56628f4a-0384-426c-b849-81ab1671afa4"),
                columns: new[] { "IsRequired", "SurveyConfigId" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("651414ad-cbcf-462a-84b9-daf83880b25a"),
                columns: new[] { "IsRequired", "SurveyConfigId" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("68c17fa9-1d90-4eb2-8412-ea5062ad26f0"),
                columns: new[] { "IsRequired", "SurveyConfigId" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("69b1a8ab-2811-450d-b9bf-12cd91e44193"),
                columns: new[] { "IsRequired", "SurveyConfigId" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("6fe5a134-6379-4244-96f1-69cc71962368"),
                columns: new[] { "IsRequired", "SurveyConfigId" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("713d8bb9-cdbe-42e7-aa16-35a1915c3532"),
                columns: new[] { "IsRequired", "SurveyConfigId" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("721d8bb9-cdbe-42e7-aa16-35a1915c1123"),
                columns: new[] { "IsRequired", "SurveyConfigId" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("73a7a0c8-1e07-4351-b3f4-3e843623bbaf"),
                columns: new[] { "IsRequired", "SurveyConfigId" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("73ab87b1-c0ae-40c6-ba37-1cdce350563a"),
                columns: new[] { "IsRequired", "SurveyConfigId" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("763d8bb9-cdbe-42e7-aa16-35a1915c3512"),
                columns: new[] { "IsRequired", "SurveyConfigId" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("83a5b867-c3a1-47b9-91e3-a6fd6086501e"),
                columns: new[] { "IsRequired", "SurveyConfigId" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("92d64860-c647-4359-a326-78a39de366e1"),
                columns: new[] { "IsRequired", "SurveyConfigId" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("9619b7d3-b2ab-4462-9044-b443ab79942a"),
                columns: new[] { "IsRequired", "SurveyConfigId" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("98a97cf1-8562-4828-9cf2-6f5823bf09fe"),
                columns: new[] { "IsRequired", "SurveyConfigId" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("a6f6b5dc-c72a-496c-859c-2fdc9d97f147"),
                columns: new[] { "IsRequired", "SurveyConfigId" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("b223125a-a4e1-4e10-b4dd-cfcd747d74c5"),
                columns: new[] { "IsRequired", "SurveyConfigId" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("bb50bb8b-4e72-4a5b-a058-cdbc7837f4dc"),
                columns: new[] { "IsRequired", "SurveyConfigId" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("c6bda257-0757-4290-86c1-794d0ba6ae3b"),
                columns: new[] { "IsRequired", "SurveyConfigId" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("c88e157b-1496-4aae-b81f-dd0718524e38"),
                columns: new[] { "IsRequired", "SurveyConfigId" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("cedef58c-7d94-4efc-a030-0216b3034bef"),
                columns: new[] { "IsRequired", "SurveyConfigId" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("da618b05-f9b8-4bfd-8bae-7a5adb668d16"),
                columns: new[] { "IsRequired", "SurveyConfigId" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("eb9ddecf-15fc-4393-af86-eb2b14f64eb7"),
                columns: new[] { "IsRequired", "SurveyConfigId" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("ee0e74f5-83ae-44dd-a7d0-0f7b650884f8"),
                columns: new[] { "IsRequired", "SurveyConfigId" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("ef128343-2e29-4735-82f7-9d8d2231dcc2"),
                columns: new[] { "IsRequired", "SurveyConfigId" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("f05b02dc-6393-41dd-a479-cc9c05d1da93"),
                columns: new[] { "IsRequired", "SurveyConfigId" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("f563da40-d609-4922-90b9-44e4290edfef"),
                columns: new[] { "IsRequired", "SurveyConfigId" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "SurveyQuestions",
                keyColumn: "Id",
                keyValue: new Guid("f959d3d1-cab3-4fc6-8341-cb44bcbd3e30"),
                columns: new[] { "IsRequired", "SurveyConfigId" },
                values: new object[] { null, null });

            migrationBuilder.CreateIndex(
                name: "IX_SurveyQuestions_SurveyConfigId",
                table: "SurveyQuestions",
                column: "SurveyConfigId");

            migrationBuilder.AddForeignKey(
                name: "FK_SurveyQuestions_SurveyConfigs_SurveyConfigId",
                table: "SurveyQuestions",
                column: "SurveyConfigId",
                principalTable: "SurveyConfigs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SurveyQuestions_SurveyConfigs_SurveyConfigId",
                table: "SurveyQuestions");

            migrationBuilder.DropTable(
                name: "SurveyConfigs");

            migrationBuilder.DropIndex(
                name: "IX_SurveyQuestions_SurveyConfigId",
                table: "SurveyQuestions");

            migrationBuilder.DropColumn(
                name: "IsRequired",
                table: "SurveyQuestions");

            migrationBuilder.DropColumn(
                name: "SurveyConfigId",
                table: "SurveyQuestions");
        }
    }
}
