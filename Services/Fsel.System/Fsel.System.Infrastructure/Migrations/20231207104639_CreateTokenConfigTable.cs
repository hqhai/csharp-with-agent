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
                    { new Guid("0b750040-7d3a-41d2-827a-935599f0db2e"), "{\"phase 1\":\"10\",\"phase 2\":\"20\",\"phase 3\":\"30\",\"phase 4\":\"40\",\"phase 5\":\"50\",\"phase 6\":\"60\"}", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Achievement", false, "CompletePhaseAchievement", "null", null, null, null },
                    { new Guid("13b22301-cc43-4cea-b82d-dfa78a7083da"), "{\"token\":\"0\"}", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "TimeCode", false, "SuperFireMode", "{\"token\":\"1\"}", null, null, null },
                    { new Guid("1904cafe-abab-43d4-95a9-b4df18c71858"), "{\"streak\":\"2\",\"token\":\"1\"}", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Test", false, "HighestStreak", "{\"streak\":\"2\",\"token\":\"2\"}", null, null, null },
                    { new Guid("2a4585ad-7155-47f8-a985-f06e32cd02b7"), "{\"unitTest/skillTest/skillMockTest\":\"10\",\"finalTest/FullMockTest\":\"40\"}", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Test", false, "TestDone", "{\"unitTest/skillTest/skillMockTest\":\"15\",\"finalTest/FullMockTest\":\"60\"}", null, null, null },
                    { new Guid("2ab68ffd-9969-4fd0-a3f1-e8d272122feb"), "{\"dailyQuest\":\"1\",\"mainQuest\":\"10\",\"sideQuest\":\"5\",\"permium\":\"10\"}", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "QuestBoard", false, "DailyMission", "null", null, null, null },
                    { new Guid("349335a6-6dca-436e-b0ea-40fe8c026b64"), "{\"unitTest/skillTest/skillMockTest\":\"10\",\"finalTest/FullMockTest\":\"40\"}", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Test", false, "TestComplete", "{\"unitTest/skillTest/skillMockTest\":\"15\",\"finalTest/FullMockTest\":\"60\"}", null, null, null },
                    { new Guid("418e095e-a79b-4b16-a212-013027ffe3f8"), "[{\"token\":\"5\",\"tier\":\"3\"},{\"token\":\"20\",\"tier\":\"15\"},{\"token\":\"40\",\"tier\":\"30-31\"}]", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "DailyCheckin", false, "DailyCheckin", "null", null, null, null },
                    { new Guid("541d077d-9a7b-4074-8240-877591957d92"), "{\"token\":\"1\",\"Streak\":\"2\"}", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Test", false, "HighestStreak", "{\"token\":\"2\",\"subquestion\":\"2\"}", null, null, null },
                    { new Guid("570f3b94-8cb0-4e30-9483-e543b863e4c8"), "{\"token\":\"0\"}", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "TimeCode", false, "SuperFireMode", "[{\"TimeCode\":\"1\",\"Test\":\"4\",\"FinalTest/MockTest\":\"10\"}]", null, null, null },
                    { new Guid("5fd4a7a7-68cf-4d44-89c9-bb6850242eda"), "{\"Token\":\"10\"}", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "ReviewSystem", false, "ReviewFSEL", "null", null, null, null },
                    { new Guid("631bb9bb-79d4-4eaf-9395-39c8c99581b1"), "{\"token\":[{\"token\":\"1\",\"times\":\"30p\"},{\"token\":\"3\",\"times\":\"60p\"},{\"token\":\"6\",\"times\":\"90p\"},{\"token\":\"12\",\"times\":\"120p\"},{\"token\":\"24\",\"times\":\"180p\"}]}", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "DailyCheckin", false, "DailyCheckin", "{\"token\":[{\"token\":\"2\",\"times\":\"30p\"},{\"token\":\"6\",\"times\":\"60p\"},{\"token\":\"12\",\"times\":\"90p\"},{\"token\":\"24\",\"times\":\"120p\"},{\"token\":\"48\",\"times\":\"180p\"}]}", null, null, null },
                    { new Guid("7b0dae34-5312-46b1-907e-a8499a532d09"), "{\"Token\":\"50\"}", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Price", false, "UpgradeCourse", "null", null, null, null },
                    { new Guid("9d3a3ad7-64a8-4469-beed-f81ec0903d43"), "{\"token\":\"0\"}", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Test", false, "SuperFireMode", "{\"unitTest/skillTest/skillMockTest\":\"4\",\"finalTest/mockTest\":\"10\"}", null, null, null },
                    { new Guid("a569629e-f6fc-424b-b1a2-51c2a826819d"), "{\"dailyQuest\":\"1\",\"mainQuest\":\"10\",\"sideQuest\":\"5\",\"permium\":\"10\"}", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "QuestBoard", false, "EventMission", "null", null, null, null },
                    { new Guid("b4732b98-4ee3-4ae6-a51b-317908973282"), "{\"mission\":\"10\"}", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Achievement", false, "CompleteAchievement", "null", null, null, null },
                    { new Guid("bb8cb9ba-58f1-43a7-9ac5-c07cd5da6ffb"), "{\"dailyQuest\":\"1\",\"mainQuest\":\"10\",\"sideQuest\":\"5\",\"permium\":\"10\"}", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "QuestBoard", false, "SideMission", "null", null, null, null },
                    { new Guid("d35c8680-c69e-41c2-bf42-cba51d2108a9"), "{\"QuestionUngraded\":\"0\",\"subQuestion\":\"1\"}", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "TimeCode", false, "QuestionReward", "{\"QuestionUngraded\":\"2\",\"subQuestion\":\"1\"}", null, null, null },
                    { new Guid("d8c548b3-17e9-4899-90a2-6ebe8fb5ef3c"), "{\"token\":\"1\",\"subquestion\":\"1\"}", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "TimeCode", false, "TimeCodeSubmit", "{\"token\":\"2\",\"subquestion\":\"1\"}", null, null, null },
                    { new Guid("e606579f-0bc8-4254-9fe2-44b0306e225c"), "{\"dailyQuest\":\"1\",\"mainQuest\":\"10\",\"sideQuest\":\"5\",\"permium\":\"10\"}", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "QuestBoard", false, "MainMission", "null", null, null, null },
                    { new Guid("fca942b7-b3bb-4ad2-8035-4471f28c56af"), "{\"standard\":\"50\",\"premium\":\"100\"}", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Price", false, "BuyCourse", "null", null, null, null }
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
