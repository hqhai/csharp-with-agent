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
                    { new Guid("061242a9-7e64-4fb1-ab8d-99b5e250c389"), "{\"number\":1}", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "FinalTest", false, "HighestStreak", "{\"number\":2}", null, null, null },
                    { new Guid("09fc6407-064e-4260-92de-12fc36c5eeda"), "{\"number\":0}", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "SkillMockTest", false, "SuperFire", "{\"number\":4}", null, null, null },
                    { new Guid("1e3c6f57-25d6-40bd-a9be-d5571be1a7ae"), "{\"number\":0}", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "FinalTest", false, "SuperFire", "{\"number\":10}", null, null, null },
                    { new Guid("272f4fdc-91d8-475b-8394-843c43c89e56"), "{\"number\":1}", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "FullMockTest", false, "HighestStreak", "{\"number\":2}", null, null, null },
                    { new Guid("394fe972-3e4b-4daf-9fcf-e6720204be24"), "{\"number\":1}", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "TimeCode", false, "QuestionReward", "{\"number\":2}", null, null, null },
                    { new Guid("5d1628c6-6c44-4afa-a911-687ae7ba753c"), "{\"number\":1}", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "QuestBoard", false, "DailyQuest", "null", null, null, null },
                    { new Guid("5e19b75e-f16c-4e44-8299-3c626804159c"), "{\"number\":10}", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "SkillTest", false, "TestDone", "{\"number\":15}", null, null, null },
                    { new Guid("65a9971f-22d0-4730-a99f-33e7ac7b3698"), "{\"number\":10}", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "QuestBoard", false, "PermiumQuest", "null", null, null, null },
                    { new Guid("6915a722-1268-45b8-adee-9d038799b020"), "{\"number\":1}", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "SkillTest", false, "HighestStreak", "{\"number\":2}", null, null, null },
                    { new Guid("6dd62758-60c3-42cc-88eb-9acb246ebda5"), "{\"number\":0}", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "FullMockTest", false, "SuperFire", "{\"number\":10}", null, null, null },
                    { new Guid("8042aeda-9f73-4981-8262-eb70f993a92c"), "{\"number\":50}", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "BuyNextCourse", false, "Standard", "null", null, null, null },
                    { new Guid("89f2e48c-308a-411f-9950-441af87d861e"), "{\"number\":0}", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "SkillTest", false, "SuperFire", "{\"number\":4}", null, null, null },
                    { new Guid("928b4f17-be50-4798-8657-54e93d3cf450"), "{\"focusTimes\":[{\"number\":1,\"focustimeid\":\"0A7B57F3-C964-4F1B-8986-DF1579C5D08B\"},{\"number\":3,\"focustimeid\":\"9B4FA7B6-1AF4-458B-82D9-621C1A88654A\"},{\"number\":6,\"focustimeid\":\"124F4341-4C87-4E5F-BA4D-2481D8D36737\"},{\"number\":12,\"focustimeid\":\"82061293-C9D0-4598-99F1-8DFD8162B999\"},{\"number\":24,\"focustimeid\":\"FD7E66D3-A29B-4DA7-BB29-2283536D836A\"}]}", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "FocusMode", false, "FocusTime", "{\"focusTimes\":[{\"number\":2,\"focustimeid\":\"0A7B57F3-C964-4F1B-8986-DF1579C5D08B\"},{\"number\":6,\"focustimeid\":\"9B4FA7B6-1AF4-458B-82D9-621C1A88654A\"},{\"number\":12,\"focustimeid\":\"124F4341-4C87-4E5F-BA4D-2481D8D36737\"},{\"number\":24,\"focustimeid\":\"82061293-C9D0-4598-99F1-8DFD8162B999\"},{\"number\":48,\"focustimeid\":\"FD7E66D3-A29B-4DA7-BB29-2283536D836A\"}]}", null, null, null },
                    { new Guid("95c9f9b7-cc16-43af-b32e-4ee3689d4e27"), "[{\"level\":1,\"number\":5},{\"level\":2,\"number\":20},{\"level\":3,\"number\":40}]", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "DailyCheckin", false, "DailyCheckin", "null", null, null, null },
                    { new Guid("a2457d1c-2331-4624-8f94-2530bb5b21e5"), "{\"number\":40}", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "FullMockTest", false, "TestDone", "{\"number\":60}", null, null, null },
                    { new Guid("b2dd2846-a565-438f-aa5f-e3af66f191aa"), "{\"number\":50}", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "UpGradeCourse", false, "UpgradeCourse", "null", null, null, null },
                    { new Guid("cb48fa8e-b45f-4599-a87f-971668bf8907"), "{\"number\":100}", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "BuyNextCourse", false, "Permium", "null", null, null, null },
                    { new Guid("ce5000ba-788c-4872-b5e1-46d582c6f596"), "{\"number\":5}", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "QuestBoard", false, "SideQuest", "null", null, null, null },
                    { new Guid("cfeefec6-e53e-4cb6-a341-5f945e94a470"), "{\"number\":1}", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "SkillMockTest", false, "HighestStreak", "{\"number\":2}", null, null, null },
                    { new Guid("d3340516-ab0b-471e-8117-2b6fd6ec1680"), "{\"number\":10}", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "QuestBoard", false, "MainQuest", "null", null, null, null },
                    { new Guid("d33de316-efe4-46d7-89e8-d27d49e8186a"), "{\"number\":10}", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "ReviewSystem", false, "ReviewCourse", "null", null, null, null },
                    { new Guid("e00c5780-7d72-4cfe-872e-5b8954cff205"), "{\"number\":10}", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "SkillMockTest", false, "TestDone", "{\"number\":15}", null, null, null },
                    { new Guid("e5361e72-412e-4022-bc66-c813c56f1b10"), "{\"number\":10}", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "ReviewSystem", false, "ReviewFSEL", "null", null, null, null },
                    { new Guid("e7bc1ee5-9151-453b-a369-aafffa8dac53"), "{\"number\":10}", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "UnitTest", false, "TestDone", "{\"number\":15}", null, null, null },
                    { new Guid("ef5dce88-eb01-4d9f-98b5-ef709bc736f7"), "{\"number\":0}", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "UnitTest", false, "SuperFire", "{\"number\":4}", null, null, null },
                    { new Guid("f0f20824-1354-4267-bf44-812455aa85d0"), "{\"number\":40}", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "FinalTest", false, "TestDone", "{\"number\":60}", null, null, null },
                    { new Guid("f1657e49-02df-44d8-aba1-46a42654cf40"), "{\"number\":1}", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "UnitTest", false, "HighestStreak", "{\"number\":2}", null, null, null }
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
