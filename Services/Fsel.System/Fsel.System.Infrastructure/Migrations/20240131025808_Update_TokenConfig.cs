using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Fsel.System.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_TokenConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "TokenConfigs",
                keyColumn: "Id",
                keyValue: new Guid("061242a9-7e64-4fb1-ab8d-99b5e250c389"));

            migrationBuilder.DeleteData(
                table: "TokenConfigs",
                keyColumn: "Id",
                keyValue: new Guid("09fc6407-064e-4260-92de-12fc36c5eeda"));

            migrationBuilder.DeleteData(
                table: "TokenConfigs",
                keyColumn: "Id",
                keyValue: new Guid("1e3c6f57-25d6-40bd-a9be-d5571be1a7ae"));

            migrationBuilder.DeleteData(
                table: "TokenConfigs",
                keyColumn: "Id",
                keyValue: new Guid("272f4fdc-91d8-475b-8394-843c43c89e56"));

            migrationBuilder.DeleteData(
                table: "TokenConfigs",
                keyColumn: "Id",
                keyValue: new Guid("394fe972-3e4b-4daf-9fcf-e6720204be24"));

            migrationBuilder.DeleteData(
                table: "TokenConfigs",
                keyColumn: "Id",
                keyValue: new Guid("5d1628c6-6c44-4afa-a911-687ae7ba753c"));

            migrationBuilder.DeleteData(
                table: "TokenConfigs",
                keyColumn: "Id",
                keyValue: new Guid("5e19b75e-f16c-4e44-8299-3c626804159c"));

            migrationBuilder.DeleteData(
                table: "TokenConfigs",
                keyColumn: "Id",
                keyValue: new Guid("65a9971f-22d0-4730-a99f-33e7ac7b3698"));

            migrationBuilder.DeleteData(
                table: "TokenConfigs",
                keyColumn: "Id",
                keyValue: new Guid("6915a722-1268-45b8-adee-9d038799b020"));

            migrationBuilder.DeleteData(
                table: "TokenConfigs",
                keyColumn: "Id",
                keyValue: new Guid("6dd62758-60c3-42cc-88eb-9acb246ebda5"));

            migrationBuilder.DeleteData(
                table: "TokenConfigs",
                keyColumn: "Id",
                keyValue: new Guid("8042aeda-9f73-4981-8262-eb70f993a92c"));

            migrationBuilder.DeleteData(
                table: "TokenConfigs",
                keyColumn: "Id",
                keyValue: new Guid("89f2e48c-308a-411f-9950-441af87d861e"));

            migrationBuilder.DeleteData(
                table: "TokenConfigs",
                keyColumn: "Id",
                keyValue: new Guid("928b4f17-be50-4798-8657-54e93d3cf450"));

            migrationBuilder.DeleteData(
                table: "TokenConfigs",
                keyColumn: "Id",
                keyValue: new Guid("95c9f9b7-cc16-43af-b32e-4ee3689d4e27"));

            migrationBuilder.DeleteData(
                table: "TokenConfigs",
                keyColumn: "Id",
                keyValue: new Guid("a2457d1c-2331-4624-8f94-2530bb5b21e5"));

            migrationBuilder.DeleteData(
                table: "TokenConfigs",
                keyColumn: "Id",
                keyValue: new Guid("b2dd2846-a565-438f-aa5f-e3af66f191aa"));

            migrationBuilder.DeleteData(
                table: "TokenConfigs",
                keyColumn: "Id",
                keyValue: new Guid("cb48fa8e-b45f-4599-a87f-971668bf8907"));

            migrationBuilder.DeleteData(
                table: "TokenConfigs",
                keyColumn: "Id",
                keyValue: new Guid("ce5000ba-788c-4872-b5e1-46d582c6f596"));

            migrationBuilder.DeleteData(
                table: "TokenConfigs",
                keyColumn: "Id",
                keyValue: new Guid("cfeefec6-e53e-4cb6-a341-5f945e94a470"));

            migrationBuilder.DeleteData(
                table: "TokenConfigs",
                keyColumn: "Id",
                keyValue: new Guid("d3340516-ab0b-471e-8117-2b6fd6ec1680"));

            migrationBuilder.DeleteData(
                table: "TokenConfigs",
                keyColumn: "Id",
                keyValue: new Guid("d33de316-efe4-46d7-89e8-d27d49e8186a"));

            migrationBuilder.DeleteData(
                table: "TokenConfigs",
                keyColumn: "Id",
                keyValue: new Guid("e00c5780-7d72-4cfe-872e-5b8954cff205"));

            migrationBuilder.DeleteData(
                table: "TokenConfigs",
                keyColumn: "Id",
                keyValue: new Guid("e7bc1ee5-9151-453b-a369-aafffa8dac53"));

            migrationBuilder.DeleteData(
                table: "TokenConfigs",
                keyColumn: "Id",
                keyValue: new Guid("ef5dce88-eb01-4d9f-98b5-ef709bc736f7"));

            migrationBuilder.DeleteData(
                table: "TokenConfigs",
                keyColumn: "Id",
                keyValue: new Guid("f0f20824-1354-4267-bf44-812455aa85d0"));

            migrationBuilder.DeleteData(
                table: "TokenConfigs",
                keyColumn: "Id",
                keyValue: new Guid("f1657e49-02df-44d8-aba1-46a42654cf40"));

            migrationBuilder.DropColumn(
                name: "SuperConfigStr",
                table: "TokenConfigs");

            migrationBuilder.AddColumn<string>(
                name: "CourseType",
                table: "TokenConfigs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "TokenConfigs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "QuestBoardConfigs",
                keyColumn: "Id",
                keyValue: new Guid("2ea59046-ea24-4f3f-b390-36d9629ae11e"),
                column: "MaxPoints",
                value: 2);

            migrationBuilder.UpdateData(
                table: "QuestBoardConfigs",
                keyColumn: "Id",
                keyValue: new Guid("cfce3f4b-66d5-467d-8f57-50a089257bcb"),
                column: "Category",
                value: "FinishOneLesson");

            migrationBuilder.UpdateData(
                table: "TokenConfigs",
                keyColumn: "Id",
                keyValue: new Guid("e5361e72-412e-4022-bc66-c813c56f1b10"),
                columns: new[] { "ConfigStr", "CourseType", "DisplayOrder", "Feature", "Mission" },
                values: new object[] { "{\"baseValue\":2,\"description\":\"When users complete each question/sub question, they will receive coins for each question/sub question\",\"totalActions\":2160}", "Academic", 1, "Learn", "TimeCodeFirstSubmit" });

            migrationBuilder.InsertData(
                table: "TokenConfigs",
                columns: new[] { "Id", "ConfigStr", "CourseType", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "DisplayOrder", "Feature", "IsDeleted", "Mission", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[,]
                {
                    { new Guid("03a07cf2-6f6e-4971-9b52-d3a7330eec2f"), "{\"baseValue\":1,\"description\":\"When users complete each question/sub question, they will receive coins for each question/sub question\",\"totalActions\":1280}", "Ielts", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 9, "Learn", false, "TimeCodeSecondSubmit", null, null, null },
                    { new Guid("0b6208a1-28b8-4721-af0b-f8298e373e91"), "{\"baseValue\":3,\"description\":\"When users complete each question/sub question, they will receive coins for each correct question/sub question\",\"totalActions\":200}", "Ielts", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 26, "Test", false, "FullMockTest", null, null, null },
                    { new Guid("191471af-a9ab-4571-a868-74b33a77f125"), "{\"baseValue\":3,\"description\":\"When users complete each question/sub question, they will receive coins for each correct question/sub question\",\"totalActions\":80}", "Ielts", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 22, "Test", false, "SkillMockTestSpeaking", null, null, null },
                    { new Guid("24e0c9f4-f4f7-4592-bd04-7f03d33847f4"), "[{\"level\":1,\"baseValue\":10,\"description\":\"user checks in for 7 days in a row\",\"totalActions\":12},{\"level\":2,\"baseValue\":30,\"description\":\"user checks in for 14 days in a row\",\"totalActions\":12},{\"level\":3,\"baseValue\":100,\"description\":\"user checks in for 30 days in a row\",\"totalActions\":12}]", "Ielts", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 18, "DailyCheckin", false, "DailyCheckin", null, null, null },
                    { new Guid("41be49bc-d0f5-481c-9298-51cb9613262f"), "{\"baseValue\":2,\"description\":\"When users complete each question/sub question, they will receive coins for each question/sub question\",\"totalActions\":1280}", "Ielts", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 8, "Learn", false, "TimeCodeFirstSubmit", null, null, null },
                    { new Guid("45a943af-08d0-4886-9479-34eaa32500b1"), "{\"baseValue\":3,\"description\":\"When users complete each question/sub question, they will receive coins for each correct question/sub question\",\"totalActions\":80}", "Ielts", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 25, "Test", false, "SkillMockTestListening", null, null, null },
                    { new Guid("45ff5759-8c5d-4cc6-87f4-ef0a13577e3c"), "[{\"level\":1,\"baseValue\":10,\"description\":\"user checks in for 7 days in a row\",\"totalActions\":12},{\"level\":2,\"baseValue\":30,\"description\":\"user checks in for 14 days in a row\",\"totalActions\":12},{\"level\":3,\"baseValue\":100,\"description\":\"user checks in for 30 days in a row\",\"totalActions\":12}]", "Academic", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 17, "DailyCheckin", false, "DailyCheckin", null, null, null },
                    { new Guid("464577de-eb10-4bd6-8a82-0a0101821675"), "{\"baseValue\":3,\"description\":\"When users complete each question/sub question, they will receive coins for each correct question/sub question\",\"totalActions\":600}", "Academic", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 20, "Test", false, "UnitTest", null, null, null },
                    { new Guid("48c53c52-fcf5-4715-8edf-35c343663708"), "{\"baseValue\":1,\"description\":\"When the user clicks submit homework, they immediately receive Coin when the HW Result Report screen appears, count by \",\"totalActions\":1080}", "Academic", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 7, "Learn", false, "HomeworkSecondSubmit", null, null, null },
                    { new Guid("4959d4a0-b063-40d9-90b2-983d11853e60"), "{\"baseValue\":2,\"description\":\"When the user clicks submit homework, they immediately receive Coin when the HW Result Report screen appears, count by \",\"totalActions\":1080}", "Academic", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 6, "Learn", false, "HomeworkFirstSubmit", null, null, null },
                    { new Guid("550d36c4-eeb5-4fed-95d0-0b12e7dc83d9"), "{\"baseValue\":3,\"description\":\"When users complete each question/sub question, they will receive coins for each correct question/sub question\",\"totalActions\":100}", "Academic", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 21, "Test", false, "FinalTest", null, null, null },
                    { new Guid("579f2fab-75fb-422f-95c3-ff50577fcbb8"), "{\"baseValue\":40,\"description\":\"Complete one Phase in Achievement\",\"totalActions\":1}", "Ielts", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 38, "Achievement", false, "PhaseIV", null, null, null },
                    { new Guid("5868a03a-8624-4ebc-b553-6d6b2034924b"), "{\"baseValue\":30,\"description\":\"When a user posts on the class forum, they will receive Coin, only the first attempt will be counted (Reposting/ second attempt will not be counted).\",\"totalActions\":36}", "Academic", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 5, "Learn", false, "ClassForumSpeakingVideo", null, null, null },
                    { new Guid("5e550e64-1acc-41f9-ba80-5a29e71d9e11"), "{\"baseValue\":20,\"description\":\"When a user posts on the class forum, they will receive Coin, only the first attempt will be counted (Reposting/ second attempt will not be counted).\",\"totalActions\":16}", "Ielts", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 10, "Learn", false, "ClassForumWriting", null, null, null },
                    { new Guid("5e5be781-893e-4f3c-8878-0ccada255a8e"), "{\"baseValue\":50,\"description\":\"Complete one Phase in Achievement\",\"totalActions\":1}", "Academic", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 32, "Achievement", false, "PhaseV", null, null, null },
                    { new Guid("5e802055-5c1e-4554-a772-bca241d486b4"), "{\"baseValue\":10,\"description\":\"Complete one Phase in Achievement\",\"totalActions\":1}", "Ielts", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 35, "Achievement", false, "PhaseI", null, null, null },
                    { new Guid("62f66584-b41a-4d09-ad88-6cef530d6734"), "{\"baseValue\":20,\"description\":\"Complete one Phase in Achievement\",\"totalActions\":1}", "Academic", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 29, "Achievement", false, "PhaseII", null, null, null },
                    { new Guid("69dd58e2-b764-4ab3-b745-cbe0c44f20f5"), "{\"baseValue\":10,\"description\":\"Complete one Phase in Achievement\",\"totalActions\":1}", "Academic", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 28, "Achievement", false, "PhaseI", null, null, null },
                    { new Guid("74b95903-f5c1-4191-98b4-199e23b68c61"), "{\"baseValue\":2,\"description\":\"When the user clicks submit homework, they immediately receive Coin when the HW Result Report screen appears, count by \",\"totalActions\":1024}", "Ielts", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 13, "Learn", false, "HomeworkFirstSubmit", null, null, null },
                    { new Guid("7a0e28c0-520b-4148-b6d3-150c70c74b50"), "{\"baseValue\":1,\"description\":\"When users complete each question/sub question, they will receive coins for each question/sub question\",\"totalActions\":2160}", "Academic", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 2, "Learn", false, "TimeCodeSecondSubmit", null, null, null },
                    { new Guid("7e3ec81c-702b-4e22-915d-89b6775794d8"), "{\"baseValue\":40,\"description\":\"Complete one Phase in Achievement\",\"totalActions\":1}", "Academic", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 31, "Achievement", false, "PhaseIV", null, null, null },
                    { new Guid("88a385bd-d532-497a-bac9-2dd0a7d72fe9"), "{\"baseValue\":50,\"description\":\"Complete one Phase in Achievement\",\"totalActions\":1}", "Ielts", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 39, "Achievement", false, "PhaseV", null, null, null },
                    { new Guid("9385bbac-01e8-4192-a244-f811bdcd6e8f"), "{\"baseValue\":2,\"description\":\"Complete mission in Achievement question list\",\"totalActions\":100}", "Ielts", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 34, "Achievement", false, "QuestionCompleted", null, null, null },
                    { new Guid("93ed1ded-3e21-4825-8b1e-2f0db7cefcbd"), "{\"baseValue\":3,\"description\":\"When users complete each question/sub question, they will receive coins for each correct question/sub question\",\"totalActions\":36}", "Ielts", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 23, "Test", false, "SkillMockTestSpeaking", null, null, null },
                    { new Guid("9dd5bbbd-69c0-4705-81f4-388bb2fd59bc"), "{\"baseValue\":1,\"description\":\"When users complete each question/sub question, they will receive coins for each question/sub question\",\"totalActions\":2160}", "Academic", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 4, "Learn", false, "ClassForumSpeakingAudio", null, null, null },
                    { new Guid("a1f9a0fb-97f8-415e-aed1-21128d6c3740"), "{\"baseValue\":20,\"description\":\"When a user posts on the class forum, they will receive Coin, only the first attempt will be counted (Reposting/ second attempt will not be counted).\",\"totalActions\":16}", "Ielts", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 11, "Learn", false, "ClassForumSpeakingAudio", null, null, null },
                    { new Guid("a2f039ce-bf5d-4777-8aa9-cacb3c40cd0a"), "{\"baseValue\":1,\"description\":\"When the user clicks submit homework, they immediately receive Coin when the HW Result Report screen appears, count by \",\"totalActions\":1024}", "Ielts", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 14, "Learn", false, "HomeworkFirstSubmit", null, null, null },
                    { new Guid("b037baa5-ef10-4746-83b7-eeb3d1cc9a56"), "{\"baseValue\":20,\"description\":\"Complete one Phase in Achievement\",\"totalActions\":1}", "Ielts", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 36, "Achievement", false, "PhaseII", null, null, null },
                    { new Guid("b900f59e-f0cb-404d-b088-14b19162572a"), "{\"baseValue\":30,\"description\":\"When a user posts on the class forum, they will receive Coin, only the first attempt will be counted (Reposting/ second attempt will not be counted).\",\"totalActions\":16}", "Ielts", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 12, "Learn", false, "ClassForumSpeakingVideo", null, null, null },
                    { new Guid("ba97013a-9ad2-44a7-8c4c-883ba04956d0"), "{\"baseValue\":20,\"description\":\"When a user posts on the class forum, they will receive Coin, only the first attempt will be counted (Reposting/ second attempt will not be counted).\",\"totalActions\":36}", "Academic", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 3, "Learn", false, "ClassForumWriting", null, null, null },
                    { new Guid("bbecaf5f-b2a5-4036-b3d5-15a02a2ce35b"), "{\"baseValue\":60,\"description\":\"Complete one Phase in Achievement\",\"totalActions\":1}", "Ielts", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 40, "Achievement", false, "PhaseVI", null, null, null },
                    { new Guid("bc261be5-f658-415a-9498-8b813742708c"), "[{\"baseValue\":5,\"focustimeid\":\"0A7B57F3-C964-4F1B-8986-DF1579C5D08B\",\"totalActions\":720,\"description\":\"User is active in focus mode 15\\u0027 continues\",\"displayOrder\":1},{\"baseValue\":15,\"focustimeid\":\"9B4FA7B6-1AF4-458B-82D9-621C1A88654A\",\"description\":\"User is active in focus mode 30\\u0027 continues\",\"totalActions\":720,\"displayOrder\":2},{\"baseValue\":25,\"focustimeid\":\"124F4341-4C87-4E5F-BA4D-2481D8D36737\",\"description\":\"User is active in focus mode 45\\u0027 continues\",\"totalActions\":720,\"displayOrder\":3},{\"baseValue\":40,\"focustimeid\":\"82061293-C9D0-4598-99F1-8DFD8162B999\",\"description\":\"User is active in focus mode 60\\u0027 continues\",\"totalActions\":720,\"displayOrder\":4},{\"baseValue\":60,\"focustimeid\":\"FD7E66D3-A29B-4DA7-BB29-2283536D836A\",\"description\":\"User is active in focus mode 90\\u0027 continues\",\"totalActions\":720,\"displayOrder\":5}]", "Academic", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 15, "FocusMode", false, "FocusMode", null, null, null },
                    { new Guid("d8020699-4aa7-45bd-8ccc-15aca3b23db5"), "{\"baseValue\":2,\"description\":\"Complete mission in Achievement question list\",\"totalActions\":100}", "Academic", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 27, "Achievement", false, "QuestionCompleted", null, null, null },
                    { new Guid("de3f2ec0-542c-4247-86e2-438f7ad0b389"), "{\"baseValue\":3,\"description\":\"When users complete each question/sub question, they will receive coins for each correct question/sub question\",\"totalActions\":360}", "Academic", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 19, "Test", false, "SkillTest", null, null, null },
                    { new Guid("e9f76d4c-ab72-4184-8092-45c6f2fecbc7"), "{\"baseValue\":30,\"description\":\"Complete one Phase in Achievement\",\"totalActions\":1}", "Ielts", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 37, "Achievement", false, "PhaseIII", null, null, null },
                    { new Guid("eb0cfb65-f451-4e66-a05f-e518e740d67e"), "{\"baseValue\":60,\"description\":\"Complete one Phase in Achievement\",\"totalActions\":1}", "Academic", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 33, "Achievement", false, "PhaseVI", null, null, null },
                    { new Guid("ee75a055-1a67-4222-a1de-b7265bc7edbb"), "{\"baseValue\":30,\"description\":\"Complete one Phase in Achievement\",\"totalActions\":1}", "Academic", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 30, "Achievement", false, "PhaseIII", null, null, null },
                    { new Guid("f08945d0-3b03-4299-ae22-e3ce2408b74b"), "[{\"baseValue\":5,\"focustimeid\":\"0A7B57F3-C964-4F1B-8986-DF1579C5D08B\",\"totalActions\":720,\"description\":\"User is active in focus mode 15\\u0027 continues\",\"displayOrder\":1},{\"baseValue\":15,\"focustimeid\":\"9B4FA7B6-1AF4-458B-82D9-621C1A88654A\",\"description\":\"User is active in focus mode 30\\u0027 continues\",\"totalActions\":720,\"displayOrder\":2},{\"baseValue\":25,\"focustimeid\":\"124F4341-4C87-4E5F-BA4D-2481D8D36737\",\"description\":\"User is active in focus mode 45\\u0027 continues\",\"totalActions\":720,\"displayOrder\":3},{\"baseValue\":40,\"focustimeid\":\"82061293-C9D0-4598-99F1-8DFD8162B999\",\"description\":\"User is active in focus mode 60\\u0027 continues\",\"totalActions\":720,\"displayOrder\":4},{\"baseValue\":60,\"focustimeid\":\"FD7E66D3-A29B-4DA7-BB29-2283536D836A\",\"description\":\"User is active in focus mode 90\\u0027 continues\",\"totalActions\":720,\"displayOrder\":5}]", "Ielts", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 16, "FocusMode", false, "FocusMode", null, null, null },
                    { new Guid("ff3046a5-40e3-48ea-bf41-194739810496"), "{\"baseValue\":3,\"description\":\"When users complete each question/sub question, they will receive coins for each correct question/sub question\",\"totalActions\":4}", "Ielts", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, 24, "Test", false, "SkillMockTestWriting", null, null, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "QuestBoardConfigs",
                keyColumn: "Id",
                keyValue: new Guid("012a3990-47bc-436f-8d7d-10fd824bd579"));

            migrationBuilder.DeleteData(
                table: "QuestBoardConfigs",
                keyColumn: "Id",
                keyValue: new Guid("0417a848-1cd0-4aef-ae33-2757652701d0"));

            migrationBuilder.DeleteData(
                table: "TokenConfigs",
                keyColumn: "Id",
                keyValue: new Guid("03a07cf2-6f6e-4971-9b52-d3a7330eec2f"));

            migrationBuilder.DeleteData(
                table: "TokenConfigs",
                keyColumn: "Id",
                keyValue: new Guid("0b6208a1-28b8-4721-af0b-f8298e373e91"));

            migrationBuilder.DeleteData(
                table: "TokenConfigs",
                keyColumn: "Id",
                keyValue: new Guid("191471af-a9ab-4571-a868-74b33a77f125"));

            migrationBuilder.DeleteData(
                table: "TokenConfigs",
                keyColumn: "Id",
                keyValue: new Guid("24e0c9f4-f4f7-4592-bd04-7f03d33847f4"));

            migrationBuilder.DeleteData(
                table: "TokenConfigs",
                keyColumn: "Id",
                keyValue: new Guid("41be49bc-d0f5-481c-9298-51cb9613262f"));

            migrationBuilder.DeleteData(
                table: "TokenConfigs",
                keyColumn: "Id",
                keyValue: new Guid("45a943af-08d0-4886-9479-34eaa32500b1"));

            migrationBuilder.DeleteData(
                table: "TokenConfigs",
                keyColumn: "Id",
                keyValue: new Guid("45ff5759-8c5d-4cc6-87f4-ef0a13577e3c"));

            migrationBuilder.DeleteData(
                table: "TokenConfigs",
                keyColumn: "Id",
                keyValue: new Guid("464577de-eb10-4bd6-8a82-0a0101821675"));

            migrationBuilder.DeleteData(
                table: "TokenConfigs",
                keyColumn: "Id",
                keyValue: new Guid("48c53c52-fcf5-4715-8edf-35c343663708"));

            migrationBuilder.DeleteData(
                table: "TokenConfigs",
                keyColumn: "Id",
                keyValue: new Guid("4959d4a0-b063-40d9-90b2-983d11853e60"));

            migrationBuilder.DeleteData(
                table: "TokenConfigs",
                keyColumn: "Id",
                keyValue: new Guid("550d36c4-eeb5-4fed-95d0-0b12e7dc83d9"));

            migrationBuilder.DeleteData(
                table: "TokenConfigs",
                keyColumn: "Id",
                keyValue: new Guid("579f2fab-75fb-422f-95c3-ff50577fcbb8"));

            migrationBuilder.DeleteData(
                table: "TokenConfigs",
                keyColumn: "Id",
                keyValue: new Guid("5868a03a-8624-4ebc-b553-6d6b2034924b"));

            migrationBuilder.DeleteData(
                table: "TokenConfigs",
                keyColumn: "Id",
                keyValue: new Guid("5e550e64-1acc-41f9-ba80-5a29e71d9e11"));

            migrationBuilder.DeleteData(
                table: "TokenConfigs",
                keyColumn: "Id",
                keyValue: new Guid("5e5be781-893e-4f3c-8878-0ccada255a8e"));

            migrationBuilder.DeleteData(
                table: "TokenConfigs",
                keyColumn: "Id",
                keyValue: new Guid("5e802055-5c1e-4554-a772-bca241d486b4"));

            migrationBuilder.DeleteData(
                table: "TokenConfigs",
                keyColumn: "Id",
                keyValue: new Guid("62f66584-b41a-4d09-ad88-6cef530d6734"));

            migrationBuilder.DeleteData(
                table: "TokenConfigs",
                keyColumn: "Id",
                keyValue: new Guid("69dd58e2-b764-4ab3-b745-cbe0c44f20f5"));

            migrationBuilder.DeleteData(
                table: "TokenConfigs",
                keyColumn: "Id",
                keyValue: new Guid("74b95903-f5c1-4191-98b4-199e23b68c61"));

            migrationBuilder.DeleteData(
                table: "TokenConfigs",
                keyColumn: "Id",
                keyValue: new Guid("7a0e28c0-520b-4148-b6d3-150c70c74b50"));

            migrationBuilder.DeleteData(
                table: "TokenConfigs",
                keyColumn: "Id",
                keyValue: new Guid("7e3ec81c-702b-4e22-915d-89b6775794d8"));

            migrationBuilder.DeleteData(
                table: "TokenConfigs",
                keyColumn: "Id",
                keyValue: new Guid("88a385bd-d532-497a-bac9-2dd0a7d72fe9"));

            migrationBuilder.DeleteData(
                table: "TokenConfigs",
                keyColumn: "Id",
                keyValue: new Guid("9385bbac-01e8-4192-a244-f811bdcd6e8f"));

            migrationBuilder.DeleteData(
                table: "TokenConfigs",
                keyColumn: "Id",
                keyValue: new Guid("93ed1ded-3e21-4825-8b1e-2f0db7cefcbd"));

            migrationBuilder.DeleteData(
                table: "TokenConfigs",
                keyColumn: "Id",
                keyValue: new Guid("9dd5bbbd-69c0-4705-81f4-388bb2fd59bc"));

            migrationBuilder.DeleteData(
                table: "TokenConfigs",
                keyColumn: "Id",
                keyValue: new Guid("a1f9a0fb-97f8-415e-aed1-21128d6c3740"));

            migrationBuilder.DeleteData(
                table: "TokenConfigs",
                keyColumn: "Id",
                keyValue: new Guid("a2f039ce-bf5d-4777-8aa9-cacb3c40cd0a"));

            migrationBuilder.DeleteData(
                table: "TokenConfigs",
                keyColumn: "Id",
                keyValue: new Guid("b037baa5-ef10-4746-83b7-eeb3d1cc9a56"));

            migrationBuilder.DeleteData(
                table: "TokenConfigs",
                keyColumn: "Id",
                keyValue: new Guid("b900f59e-f0cb-404d-b088-14b19162572a"));

            migrationBuilder.DeleteData(
                table: "TokenConfigs",
                keyColumn: "Id",
                keyValue: new Guid("ba97013a-9ad2-44a7-8c4c-883ba04956d0"));

            migrationBuilder.DeleteData(
                table: "TokenConfigs",
                keyColumn: "Id",
                keyValue: new Guid("bbecaf5f-b2a5-4036-b3d5-15a02a2ce35b"));

            migrationBuilder.DeleteData(
                table: "TokenConfigs",
                keyColumn: "Id",
                keyValue: new Guid("bc261be5-f658-415a-9498-8b813742708c"));

            migrationBuilder.DeleteData(
                table: "TokenConfigs",
                keyColumn: "Id",
                keyValue: new Guid("d8020699-4aa7-45bd-8ccc-15aca3b23db5"));

            migrationBuilder.DeleteData(
                table: "TokenConfigs",
                keyColumn: "Id",
                keyValue: new Guid("de3f2ec0-542c-4247-86e2-438f7ad0b389"));

            migrationBuilder.DeleteData(
                table: "TokenConfigs",
                keyColumn: "Id",
                keyValue: new Guid("e9f76d4c-ab72-4184-8092-45c6f2fecbc7"));

            migrationBuilder.DeleteData(
                table: "TokenConfigs",
                keyColumn: "Id",
                keyValue: new Guid("eb0cfb65-f451-4e66-a05f-e518e740d67e"));

            migrationBuilder.DeleteData(
                table: "TokenConfigs",
                keyColumn: "Id",
                keyValue: new Guid("ee75a055-1a67-4222-a1de-b7265bc7edbb"));

            migrationBuilder.DeleteData(
                table: "TokenConfigs",
                keyColumn: "Id",
                keyValue: new Guid("f08945d0-3b03-4299-ae22-e3ce2408b74b"));

            migrationBuilder.DeleteData(
                table: "TokenConfigs",
                keyColumn: "Id",
                keyValue: new Guid("ff3046a5-40e3-48ea-bf41-194739810496"));

            migrationBuilder.DropColumn(
                name: "CourseType",
                table: "TokenConfigs");

            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "TokenConfigs");

            migrationBuilder.AddColumn<string>(
                name: "SuperConfigStr",
                table: "TokenConfigs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "QuestBoardConfigs",
                keyColumn: "Id",
                keyValue: new Guid("2ea59046-ea24-4f3f-b390-36d9629ae11e"),
                column: "MaxPoints",
                value: 1);

            migrationBuilder.UpdateData(
                table: "QuestBoardConfigs",
                keyColumn: "Id",
                keyValue: new Guid("cfce3f4b-66d5-467d-8f57-50a089257bcb"),
                column: "Category",
                value: "FinishOneUnitTest");

            migrationBuilder.UpdateData(
                table: "TokenConfigs",
                keyColumn: "Id",
                keyValue: new Guid("e5361e72-412e-4022-bc66-c813c56f1b10"),
                columns: new[] { "ConfigStr", "Feature", "Mission", "SuperConfigStr" },
                values: new object[] { "{\"number\":10}", "ReviewSystem", "ReviewPlatform", "null" });

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
                    { new Guid("95c9f9b7-cc16-43af-b32e-4ee3689d4e27"), "{\"dailyCheckIns\":[{\"level\":1,\"number\":5},{\"level\":2,\"number\":20},{\"level\":3,\"number\":40}]}", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "DailyCheckin", false, "DailyCheckin", "null", null, null, null },
                    { new Guid("a2457d1c-2331-4624-8f94-2530bb5b21e5"), "{\"number\":40}", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "FullMockTest", false, "TestDone", "{\"number\":60}", null, null, null },
                    { new Guid("b2dd2846-a565-438f-aa5f-e3af66f191aa"), "{\"number\":50}", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "UpGradeCourse", false, "UpgradeCourse", "null", null, null, null },
                    { new Guid("cb48fa8e-b45f-4599-a87f-971668bf8907"), "{\"number\":100}", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "BuyNextCourse", false, "Permium", "null", null, null, null },
                    { new Guid("ce5000ba-788c-4872-b5e1-46d582c6f596"), "{\"number\":5}", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "QuestBoard", false, "SideQuest", "null", null, null, null },
                    { new Guid("cfeefec6-e53e-4cb6-a341-5f945e94a470"), "{\"number\":1}", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "SkillMockTest", false, "HighestStreak", "{\"number\":2}", null, null, null },
                    { new Guid("d3340516-ab0b-471e-8117-2b6fd6ec1680"), "{\"number\":10}", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "QuestBoard", false, "MainQuest", "null", null, null, null },
                    { new Guid("d33de316-efe4-46d7-89e8-d27d49e8186a"), "{\"number\":10}", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "ReviewSystem", false, "ReviewCourse", "null", null, null, null },
                    { new Guid("e00c5780-7d72-4cfe-872e-5b8954cff205"), "{\"number\":10}", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "SkillMockTest", false, "TestDone", "{\"number\":15}", null, null, null },
                    { new Guid("e7bc1ee5-9151-453b-a369-aafffa8dac53"), "{\"number\":10}", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "UnitTest", false, "TestDone", "{\"number\":15}", null, null, null },
                    { new Guid("ef5dce88-eb01-4d9f-98b5-ef709bc736f7"), "{\"number\":0}", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "UnitTest", false, "SuperFire", "{\"number\":4}", null, null, null },
                    { new Guid("f0f20824-1354-4267-bf44-812455aa85d0"), "{\"number\":40}", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "FinalTest", false, "TestDone", "{\"number\":60}", null, null, null },
                    { new Guid("f1657e49-02df-44d8-aba1-46a42654cf40"), "{\"number\":1}", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "UnitTest", false, "HighestStreak", "{\"number\":2}", null, null, null }
                });
        }
    }
}
