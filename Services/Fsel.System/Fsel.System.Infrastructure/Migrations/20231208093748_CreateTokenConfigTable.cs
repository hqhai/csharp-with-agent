using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Fsel.System.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateTokenConfigTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TokenConfigs",
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
                    Feature = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Mission = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ConfigStr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SuperConfigStr = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TokenConfigs", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "TokenConfigs",
                columns: new[] { "Id", "ConfigStr", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "Feature", "IsDeleted", "Mission", "SuperConfigStr", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[,]
                {
                    { new Guid("0b750040-7d3a-41d2-827a-935599f0db2e"), "[{\"id\":1,\"number\":10},{\"id\":2,\"number\":20},{\"id\":3,\"number\":30},{\"id\":4,\"number\":40},{\"id\":5,\"number\":50},{\"id\":6,\"number\":60}]", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Achievement", false, "CompletePhaseAchievement", "null", null, null, null },
                    { new Guid("13b22301-cc43-4cea-b82d-dfa78a7083da"), "{\"number\":0}", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "TimeCode", false, "SuperFireMode", "{\"number\":1}", null, null, null },
                    { new Guid("1904cafe-abab-43d4-95a9-b4df18c71858"), "{\"number\":1}", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Test", false, "HighestStreak", "{\"number\":2}", null, null, null },
                    { new Guid("2a4585ad-7155-47f8-a985-f06e32cd02b7"), "{\"test\":10,\"mockTest\":40}", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Test", false, "TestDone", "{\"test\":15,\"mockTest\":60}", null, null, null },
                    { new Guid("2ab68ffd-9969-4fd0-a3f1-e8d272122feb"), "{\"dailyQuest\":1,\"mainQuest\":10,\"sideQuest\":5,\"permium\":10}", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "QuestBoard", false, "QuestBoard", "null", null, null, null },
                    { new Guid("349335a6-6dca-436e-b0ea-40fe8c026b64"), "{\"test\":10,\"mockTest\":40}", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Test", false, "TestComplete", "{\"test\":15,\"mockTest\":60}", null, null, null },
                    { new Guid("418e095e-a79b-4b16-a212-013027ffe3f8"), "[{\"level\":1,\"number\":5},{\"level\":2,\"number\":20},{\"level\":3,\"number\":40}]", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "DailyCheckin", false, "DailyCheckin", "null", null, null, null },
                    { new Guid("541d077d-9a7b-4074-8240-877591957d92"), "{\"number\":1}", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Test", false, "HighestStreak", "{\"number\":2}", null, null, null },
                    { new Guid("570f3b94-8cb0-4e30-9483-e543b863e4c8"), "{\"timeCode\":0,\"test\":0,\"mockTest\":0}", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "TimeCode", false, "SuperFireMode", "[{\"timeCode\":1,\"test\":4,\"mockTest\":10}]", null, null, null },
                    { new Guid("5fd4a7a7-68cf-4d44-89c9-bb6850242eda"), "{\"number\":10}", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "ReviewSystem", false, "ReviewFSEL", "null", null, null, null },
                    { new Guid("631bb9bb-79d4-4eaf-9395-39c8c99581b1"), "{\"focusTimes\":[{\"number\":1,\"focustimeid\":\"0A7B57F3-C964-4F1B-8986-DF1579C5D08B\"},{\"number\":3,\"focustimeid\":\"9B4FA7B6-1AF4-458B-82D9-621C1A88654A\"},{\"number\":6,\"focustimeid\":\"124F4341-4C87-4E5F-BA4D-2481D8D36737\"},{\"number\":12,\"focustimeid\":\"82061293-C9D0-4598-99F1-8DFD8162B999\"},{\"number\":24,\"focustimeid\":\"FD7E66D3-A29B-4DA7-BB29-2283536D836A\"}]}", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "FocusMode", false, "FocusTime", "{\"token\":[{\"number\":2,\"focustimeid\":\"0A7B57F3-C964-4F1B-8986-DF1579C5D08B\"},{\"number\":6,\"focustimeid\":\"9B4FA7B6-1AF4-458B-82D9-621C1A88654A\"},{\"number\":12,\"focustimeid\":\"124F4341-4C87-4E5F-BA4D-2481D8D36737\"},{\"number\":24,\"focustimeid\":\"82061293-C9D0-4598-99F1-8DFD8162B999\"},{\"number\":48,\"focustimeid\":\"FD7E66D3-A29B-4DA7-BB29-2283536D836A\"}]}", null, null, null },
                    { new Guid("7b0dae34-5312-46b1-907e-a8499a532d09"), "{\"number\":50}", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Price", false, "UpgradeCourse", "null", null, null, null },
                    { new Guid("9d3a3ad7-64a8-4469-beed-f81ec0903d43"), "{\"test\":4,\"mockTest\":10}", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Test", false, "SuperFireMode", "{\"test\":4,\"mockTest\":10}", null, null, null },
                    { new Guid("b4732b98-4ee3-4ae6-a51b-317908973282"), "{\"number\":10}", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Achievement", false, "CompleteAchievement", "null", null, null, null },
                    { new Guid("d35c8680-c69e-41c2-bf42-cba51d2108a9"), "{\"questionUngraded\":0,\"subQuestion\":1}", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "TimeCode", false, "QuestionReward", "{\"questionUngraded\":0,\"subQuestion\":2}", null, null, null },
                    { new Guid("d8c548b3-17e9-4899-90a2-6ebe8fb5ef3c"), "{\"number\":1}", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "TimeCode", false, "TimeCodeSubmit", "{\"number\":2}", null, null, null },
                    { new Guid("fca942b7-b3bb-4ad2-8035-4471f28c56af"), "{\"numberStandard\":50,\"numberPremium\":100}", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Price", false, "BuyCourseStandad", "null", null, null, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TokenConfigs");
        }
    }
}
