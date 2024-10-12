using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Fsel.System.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTableTechieTranslation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TechieActionTranslation",
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
                    TemplateMessage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TechieActionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Language = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TechieActionTranslation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TechieActionTranslation_TechieActions_TechieActionId",
                        column: x => x.TechieActionId,
                        principalTable: "TechieActions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Techie",
                columns: new[] { "Id", "Code", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "Icon", "Image", "IsDeleted", "Name", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[] { new Guid("6d9cb3cc-8b96-4c93-8d67-cda597e6e5eb"), "Techie-01", new DateTime(2024, 6, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, null, false, "Techie", null, null, null });

            migrationBuilder.InsertData(
                table: "TechieActions",
                columns: new[] { "Id", "Action", "ConfigStr", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "Feature", "Icon", "Image", "IsDeleted", "Priority", "TechieId", "TemplateMessage", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[,]
                {
                    { new Guid("0b7c3774-bf42-4475-ba43-779d8beee88b"), "StreakFourtyFiveTimeCode", "{\"startTime\":0,\"endTime\":0,\"value\":\"45\"}", new DateTime(2024, 6, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Cheer", "string", "string", false, 3, new Guid("6d9cb3cc-8b96-4c93-8d67-cda597e6e5eb"), "Xuất sắc! {0}/{0}! 🌟✅", null, null, null },
                    { new Guid("162c976a-e869-45c9-9206-123820fa04c2"), "StreakFiftyTimeCode", "{\"startTime\":0,\"endTime\":0,\"value\":\"50\"}", new DateTime(2024, 6, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Cheer", "string", "string", false, 3, new Guid("6d9cb3cc-8b96-4c93-8d67-cda597e6e5eb"), "Không thể tin được! {0} timecode đúng liên tiếp! 🌠✨", null, null, null },
                    { new Guid("2b6fcf1f-4df4-4182-bc3e-6b1a7b7a0744"), "GoodAfternoon", "{\"startTime\":12,\"endTime\":17,\"value\":\"string\"}", new DateTime(2024, 6, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Greeting", "string", "string", false, 3, new Guid("6d9cb3cc-8b96-4c93-8d67-cda597e6e5eb"), "🚀 Chào buổi chiều, {0}! Buổi chiều đến rồi, hãy khám phá thêm nhiều bí mật của vũ trụ tri thức nhé! 🌇🌟", null, null, null },
                    { new Guid("358a5e9e-f7f1-471e-b402-53ef27688746"), "GoodEvening", "{\"startTime\":18,\"endTime\":21,\"value\":\"string\"}", new DateTime(2024, 6, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Greeting", "string", "string", false, 3, new Guid("6d9cb3cc-8b96-4c93-8d67-cda597e6e5eb"), "🚀 Chào buổi tối, {0}! Hãy thắp sáng ngọn lửa tri thức và khám phá vũ trụ nhé! 🌟✨", null, null, null },
                    { new Guid("365768fb-df0d-4bba-8aef-0ffaada02ccd"), "StreakFiveTimeCode", "{\"startTime\":0,\"endTime\":0,\"value\":\"5\"}", new DateTime(2024, 6, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Cheer", "string", "string", false, 3, new Guid("6d9cb3cc-8b96-4c93-8d67-cda597e6e5eb"), "{0} timecode liên tiếp! Cậu đang bùng cháy! 🔥🚀", null, null, null },
                    { new Guid("3fdeece5-0526-4ba5-9dc5-d8c91d172e94"), "StreakTenTimeCode", "{\"startTime\":0,\"endTime\":0,\"value\":\"10\"}", new DateTime(2024, 6, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Cheer", "string", "string", false, 3, new Guid("6d9cb3cc-8b96-4c93-8d67-cda597e6e5eb"), "{0} Time code đúng liên tiếp! Tiếp tục tiến lên! 🚀🌌", null, null, null },
                    { new Guid("78bc6adc-9a25-4b9d-8376-89a75123224e"), "GoodMorning", "{\"startTime\":7,\"endTime\":11,\"value\":\"string\"}", new DateTime(2024, 6, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Greeting", "string", "string", false, 3, new Guid("6d9cb3cc-8b96-4c93-8d67-cda597e6e5eb"), "🚀 Chào buổi sáng, {0}! Hãy để buổi sáng này bắt đầu với những khám phá mới trong vũ trụ tri thức nhé! 🌞🪐", null, null, null },
                    { new Guid("7a14752c-fd0a-43e2-9625-97b7ca4438d4"), "StreakTwentyFiveTimeCode", "{\"startTime\":0,\"endTime\":0,\"value\":\"25\"}", new DateTime(2024, 6, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Cheer", "string", "string", false, 3, new Guid("6d9cb3cc-8b96-4c93-8d67-cda597e6e5eb"), "Xuất sắc! {0}/{0}! 🌟✅", null, null, null },
                    { new Guid("7b3784b0-da4d-4ddd-9e1f-ff2806d12105"), "StreakTwentyTimeCode", "{\"startTime\":0,\"endTime\":0,\"value\":\"20\"}", new DateTime(2024, 6, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Cheer", "string", "string", false, 3, new Guid("6d9cb3cc-8b96-4c93-8d67-cda597e6e5eb"), "{0} timecode đúng liên tiếp! Cậu không thể ngăn cản! 🚀🌌", null, null, null },
                    { new Guid("93abbb9b-2b2a-4b2a-b926-20576be9f69d"), "StreakFifTeenTimeCode", "{\"startTime\":0,\"endTime\":0,\"value\":\"15\"}", new DateTime(2024, 6, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Cheer", "string", "string", false, 3, new Guid("6d9cb3cc-8b96-4c93-8d67-cda597e6e5eb"), "Xuất sắc! {0}/{0}! 🌟✅", null, null, null },
                    { new Guid("9656f061-1504-4b25-bb55-834ad90ea88f"), "StreakFourtyTimeCode", "{\"startTime\":0,\"endTime\":0,\"value\":\"40\"}", new DateTime(2024, 6, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Cheer", "string", "string", false, 3, new Guid("6d9cb3cc-8b96-4c93-8d67-cda597e6e5eb"), "{0} timecode liên tiếp! Cậu là một siêu sao! 🌌✨", null, null, null },
                    { new Guid("983aa2f9-218f-45ad-82b5-8528bcac363a"), "GoodEarlyMorning", "{\"startTime\":12,\"endTime\":17,\"value\":\"string\"}", new DateTime(2024, 6, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Greeting", "string", "string", false, 3, new Guid("6d9cb3cc-8b96-4c93-8d67-cda597e6e5eb"), "Chào ngày mới, {0}! 🌅🚀Wow, dậy sớm thế này, bạn chắc chắn là người đầu tiên ngắm mặt trời mọc đó. Thật chăm chỉ quá đi!", null, null, null },
                    { new Guid("b34cfdc1-5bc0-4a00-8d53-b5b88e932a1d"), "GoodLateNight", "{\"startTime\":22,\"endTime\":2,\"value\":\"string\"}", new DateTime(2024, 6, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Greeting", "string", "string", false, 3, new Guid("6d9cb3cc-8b96-4c93-8d67-cda597e6e5eb"), "Chào buổi đêm, {0}! 🌙🚀 Đêm khuya rồi mà bạn vẫn học ư?? Sự nỗ lực của bạn đang toả sáng như các ngôi sao trên bầu trời đó! Hãy tiếp tục tỏa sáng và khám phá thêm nhiều điều thú vị nhé! 🌟📚✨", null, null, null },
                    { new Guid("c90fcc08-4e4f-45d9-a8aa-3a147091cd38"), "StreakThirtyTimeCode", "{\"startTime\":0,\"endTime\":0,\"value\":\"30\"}", new DateTime(2024, 6, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Cheer", "string", "string", false, 3, new Guid("6d9cb3cc-8b96-4c93-8d67-cda597e6e5eb"), "{0} timecode liên tiếp! Phi thường! 🌟🚀", null, null, null },
                    { new Guid("f23cdc48-3d66-43b6-8b35-1ffa696bd5e7"), "StreakThirtyFiveTimeCode", "{\"startTime\":0,\"endTime\":0,\"value\":\"35\"}", new DateTime(2024, 6, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Cheer", "string", "string", false, 3, new Guid("6d9cb3cc-8b96-4c93-8d67-cda597e6e5eb"), "Kỳ diệu! {0} timecode đúng! 🌟💥", null, null, null }
                });

            migrationBuilder.InsertData(
                table: "TechieActionTranslation",
                columns: new[] { "Id", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "IsDeleted", "Language", "TechieActionId", "TemplateMessage", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[,]
                {
                    { new Guid("00bee779-ac7d-4457-b702-901684eaff2b"), new DateTime(2024, 6, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "vi-VN", new Guid("7b3784b0-da4d-4ddd-9e1f-ff2806d12105"), "{0} timecode đúng liên tiếp! Cậu không thể ngăn cản! 🚀🌌", null, null, null },
                    { new Guid("0164e974-ef84-4745-b2d4-7e949625176f"), new DateTime(2024, 6, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "en-US", new Guid("983aa2f9-218f-45ad-82b5-8528bcac363a"), "Good morning, {0}! 🌅🚀 Wow, you're up early! You must be the first to see the sunrise. Such dedication!", null, null, null },
                    { new Guid("16a3cf13-3cf3-4104-a716-dcf3dee037c8"), new DateTime(2024, 6, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "en-US", new Guid("f23cdc48-3d66-43b6-8b35-1ffa696bd5e7"), "Amazing! {0} correct timecodes! 🌟💥", null, null, null },
                    { new Guid("2616c223-14c2-424f-b772-446869de15c1"), new DateTime(2024, 6, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "en-US", new Guid("c90fcc08-4e4f-45d9-a8aa-3a147091cd38"), "{0} correct timecodes in a row! Extraordinary! 🌟🚀", null, null, null },
                    { new Guid("287aa284-5f84-40cb-9af5-0269d0ecd8e1"), new DateTime(2024, 6, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "vi-VN", new Guid("983aa2f9-218f-45ad-82b5-8528bcac363a"), "Chào ngày mới, {0}! 🌅🚀Wow, dậy sớm thế này, bạn chắc chắn là người đầu tiên ngắm mặt trời mọc đó. Thật chăm chỉ quá đi!", null, null, null },
                    { new Guid("2e434057-6b06-4536-a0d5-851ab71cc55b"), new DateTime(2024, 6, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "vi-VN", new Guid("f23cdc48-3d66-43b6-8b35-1ffa696bd5e7"), "Kỳ diệu! {0} timecode đúng! 🌟💥", null, null, null },
                    { new Guid("3ee73930-5d3e-4b53-bf2d-a02bc9f094e0"), new DateTime(2024, 6, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "en-US", new Guid("358a5e9e-f7f1-471e-b402-53ef27688746"), "🚀 Good evening, {0}! Ignite the flame of knowledge and explore the universe! 🌟✨", null, null, null },
                    { new Guid("3fa8733d-4ebb-4c58-9351-81ed69a6fb3e"), new DateTime(2024, 6, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "vi-VN", new Guid("2b6fcf1f-4df4-4182-bc3e-6b1a7b7a0744"), "🚀 Chào buổi chiều, {0}! Buổi chiều đến rồi, hãy khám phá thêm nhiều bí mật của vũ trụ tri thức nhé! 🌇🌟", null, null, null },
                    { new Guid("40c64b95-e760-4617-9aa5-021f74669c75"), new DateTime(2024, 6, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "en-US", new Guid("0b7c3774-bf42-4475-ba43-779d8beee88b"), "Outstanding! {0}/{0}! 🌟✅", null, null, null },
                    { new Guid("50f53f4c-4cdf-4b46-8644-5613a5c28a7e"), new DateTime(2024, 6, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "en-US", new Guid("b34cfdc1-5bc0-4a00-8d53-b5b88e932a1d"), "Good night, {0}! 🌙🚀 Still studying late into the night? Your effort shines like the stars in the sky! Keep shining and discover more fascinating things! 🌟📚✨", null, null, null },
                    { new Guid("51af7570-fc5b-42b1-bcba-47740d579639"), new DateTime(2024, 6, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "vi-VN", new Guid("b34cfdc1-5bc0-4a00-8d53-b5b88e932a1d"), "Chào buổi đêm, {0}! 🌙🚀 Đêm khuya rồi mà bạn vẫn học ư?? Sự nỗ lực của bạn đang toả sáng như các ngôi sao trên bầu trời đó! Hãy tiếp tục tỏa sáng và khám phá thêm nhiều điều thú vị nhé! 🌟📚✨", null, null, null },
                    { new Guid("5a72a596-0390-4f85-a3c2-a16eaee9d883"), new DateTime(2024, 6, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "en-US", new Guid("162c976a-e869-45c9-9206-123820fa04c2"), "Unbelievable! {0} correct timecodes in a row! 🌠✨", null, null, null },
                    { new Guid("5b065c13-dc80-419f-a5dc-cf11e7bad5a6"), new DateTime(2024, 6, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "en-US", new Guid("365768fb-df0d-4bba-8aef-0ffaada02ccd"), "{0} correct timecodes in a row! You're on fire! 🔥🚀", null, null, null },
                    { new Guid("5c02e53d-0cd2-4550-ac64-508335703be6"), new DateTime(2024, 6, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "vi-VN", new Guid("9656f061-1504-4b25-bb55-834ad90ea88f"), "{0} timecode liên tiếp! Cậu là một siêu sao! 🌌✨", null, null, null },
                    { new Guid("645ea67c-460b-4619-821a-36e9657e0b62"), new DateTime(2024, 6, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "en-US", new Guid("7a14752c-fd0a-43e2-9625-97b7ca4438d4"), "Superb! {0}/{0}! 🌟✅", null, null, null },
                    { new Guid("66797936-1421-462a-bd0c-6e6ac893a4f3"), new DateTime(2024, 6, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "vi-VN", new Guid("358a5e9e-f7f1-471e-b402-53ef27688746"), "🚀 Good evening, {0}! Ignite the flame of knowledge and explore the universe! 🌟✨", null, null, null },
                    { new Guid("66a44e42-66cd-4eb6-823c-2bac5246d659"), new DateTime(2024, 6, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "en-US", new Guid("78bc6adc-9a25-4b9d-8376-89a75123224e"), "🚀 Good morning, {0}! Let's start this morning with new discoveries in the universe of knowledge! 🌞🪐", null, null, null },
                    { new Guid("70d5f258-41c3-4ea6-8392-faedeefa5d12"), new DateTime(2024, 6, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "en-US", new Guid("7b3784b0-da4d-4ddd-9e1f-ff2806d12105"), "{0} correct timecodes in a row! You're unstoppable! 🚀🌌", null, null, null },
                    { new Guid("70ea4c4b-e71c-44d2-9a68-94919324742a"), new DateTime(2024, 6, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "vi-VN", new Guid("93abbb9b-2b2a-4b2a-b926-20576be9f69d"), "Xuất sắc! {0}/{0}! 🌟✅", null, null, null },
                    { new Guid("72b29aee-4091-49da-8476-9a26136527fe"), new DateTime(2024, 6, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "vi-VN", new Guid("365768fb-df0d-4bba-8aef-0ffaada02ccd"), "{0} timecode liên tiếp! Cậu đang bùng cháy! 🔥🚀", null, null, null },
                    { new Guid("8db87c55-d96c-4d53-b762-3f783a62f620"), new DateTime(2024, 6, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "vi-VN", new Guid("78bc6adc-9a25-4b9d-8376-89a75123224e"), "🚀 Chào buổi sáng, {0}! Hãy để buổi sáng này bắt đầu với những khám phá mới trong vũ trụ tri thức nhé! 🌞🪐", null, null, null },
                    { new Guid("b1c8bda5-f279-40b6-bfa5-e3d8dc440597"), new DateTime(2024, 6, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "vi-VN", new Guid("7a14752c-fd0a-43e2-9625-97b7ca4438d4"), "Xuất sắc! {0}/{0}! 🌟✅", null, null, null },
                    { new Guid("b79bb94d-1a79-4d53-b705-b19773c4d716"), new DateTime(2024, 6, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "vi-VN", new Guid("3fdeece5-0526-4ba5-9dc5-d8c91d172e94"), "{0} Time code đúng liên tiếp! Tiếp tục tiến lên! 🚀🌌", null, null, null },
                    { new Guid("be65682b-433c-4ed7-84bb-bdeb73838ff5"), new DateTime(2024, 6, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "vi-VN", new Guid("0b7c3774-bf42-4475-ba43-779d8beee88b"), "{0} timecode liên tiếp! Cậu là một siêu sao! 🌌✨", null, null, null },
                    { new Guid("caf06cfa-4080-4242-85b5-43553a26a019"), new DateTime(2024, 6, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "en-US", new Guid("93abbb9b-2b2a-4b2a-b926-20576be9f69d"), "Excellent! {0}/{0}! 🌟✅", null, null, null },
                    { new Guid("df298010-6c86-43f8-af5e-ef7596fcf66f"), new DateTime(2024, 6, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "vi-VN", new Guid("c90fcc08-4e4f-45d9-a8aa-3a147091cd38"), "{0} timecode liên tiếp! Phi thường! 🌟🚀", null, null, null },
                    { new Guid("e28494b0-f034-48af-9951-cdc3cd3d64a4"), new DateTime(2024, 6, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "vi-VN", new Guid("162c976a-e869-45c9-9206-123820fa04c2"), "Không thể tin được! {0} timecode đúng liên tiếp! 🌠✨", null, null, null },
                    { new Guid("eacccfec-6c04-4889-8fc3-48c98ea243b1"), new DateTime(2024, 6, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "en-US", new Guid("9656f061-1504-4b25-bb55-834ad90ea88f"), "{0} correct timecodes in a row! You're a superstar! 🌌✨", null, null, null },
                    { new Guid("ef99e614-167b-4411-b674-6693e5380c25"), new DateTime(2024, 6, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "en-US", new Guid("2b6fcf1f-4df4-4182-bc3e-6b1a7b7a0744"), "🚀 Good afternoon, {0}! The afternoon has arrived, let's uncover more secrets of the knowledge universe! 🌇🌟", null, null, null },
                    { new Guid("fdcb7157-06ea-4d5a-8d19-883f17b1941c"), new DateTime(2024, 6, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, "en-US", new Guid("3fdeece5-0526-4ba5-9dc5-d8c91d172e94"), "{0} correct timecodes in a row! Keep pushing forward! 🚀🌌", null, null, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_TechieActionTranslation_TechieActionId",
                table: "TechieActionTranslation",
                column: "TechieActionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TechieActionTranslation");

            migrationBuilder.DeleteData(
                table: "TechieActions",
                keyColumn: "Id",
                keyValue: new Guid("0b7c3774-bf42-4475-ba43-779d8beee88b"));

            migrationBuilder.DeleteData(
                table: "TechieActions",
                keyColumn: "Id",
                keyValue: new Guid("162c976a-e869-45c9-9206-123820fa04c2"));

            migrationBuilder.DeleteData(
                table: "TechieActions",
                keyColumn: "Id",
                keyValue: new Guid("2b6fcf1f-4df4-4182-bc3e-6b1a7b7a0744"));

            migrationBuilder.DeleteData(
                table: "TechieActions",
                keyColumn: "Id",
                keyValue: new Guid("358a5e9e-f7f1-471e-b402-53ef27688746"));

            migrationBuilder.DeleteData(
                table: "TechieActions",
                keyColumn: "Id",
                keyValue: new Guid("365768fb-df0d-4bba-8aef-0ffaada02ccd"));

            migrationBuilder.DeleteData(
                table: "TechieActions",
                keyColumn: "Id",
                keyValue: new Guid("3fdeece5-0526-4ba5-9dc5-d8c91d172e94"));

            migrationBuilder.DeleteData(
                table: "TechieActions",
                keyColumn: "Id",
                keyValue: new Guid("78bc6adc-9a25-4b9d-8376-89a75123224e"));

            migrationBuilder.DeleteData(
                table: "TechieActions",
                keyColumn: "Id",
                keyValue: new Guid("7a14752c-fd0a-43e2-9625-97b7ca4438d4"));

            migrationBuilder.DeleteData(
                table: "TechieActions",
                keyColumn: "Id",
                keyValue: new Guid("7b3784b0-da4d-4ddd-9e1f-ff2806d12105"));

            migrationBuilder.DeleteData(
                table: "TechieActions",
                keyColumn: "Id",
                keyValue: new Guid("93abbb9b-2b2a-4b2a-b926-20576be9f69d"));

            migrationBuilder.DeleteData(
                table: "TechieActions",
                keyColumn: "Id",
                keyValue: new Guid("9656f061-1504-4b25-bb55-834ad90ea88f"));

            migrationBuilder.DeleteData(
                table: "TechieActions",
                keyColumn: "Id",
                keyValue: new Guid("983aa2f9-218f-45ad-82b5-8528bcac363a"));

            migrationBuilder.DeleteData(
                table: "TechieActions",
                keyColumn: "Id",
                keyValue: new Guid("b34cfdc1-5bc0-4a00-8d53-b5b88e932a1d"));

            migrationBuilder.DeleteData(
                table: "TechieActions",
                keyColumn: "Id",
                keyValue: new Guid("c90fcc08-4e4f-45d9-a8aa-3a147091cd38"));

            migrationBuilder.DeleteData(
                table: "TechieActions",
                keyColumn: "Id",
                keyValue: new Guid("f23cdc48-3d66-43b6-8b35-1ffa696bd5e7"));

            migrationBuilder.DeleteData(
                table: "Techie",
                keyColumn: "Id",
                keyValue: new Guid("6d9cb3cc-8b96-4c93-8d67-cda597e6e5eb"));
        }
    }
}
