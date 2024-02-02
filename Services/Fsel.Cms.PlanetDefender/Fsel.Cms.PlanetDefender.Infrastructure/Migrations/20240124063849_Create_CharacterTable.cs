using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Fsel.Cms.PlanetDefender.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Create_CharacterTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Gender",
                table: "StudentGameInfos");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "SpaceShips",
                type: "nvarchar(750)",
                maxLength: 750,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "Price",
                table: "SpaceShips",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<Guid>(
                name: "ZMatterId",
                table: "GameAnswers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Characters",
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
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Price = table.Column<long>(type: "bigint", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Characters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StudentCharacters",
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
                    StudentGameInfoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CharacterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentCharacters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentCharacters_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentCharacters_StudentGameInfos_StudentGameInfoId",
                        column: x => x.StudentGameInfoId,
                        principalTable: "StudentGameInfos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Characters",
                columns: new[] { "Id", "Code", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "Description", "IsDefault", "IsDeleted", "Name", "Price", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[,]
                {
                    { new Guid("098c3d4a-93bb-407e-b359-64d869abf1ba"), "COSMO", new DateTime(2024, 1, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Cosmo là tên bí danh của Ethan Van der Woodsen hay Ethan Woodsen, con trai duy nhất của cựu thống đốc bang Wisconsin – John Van der Woodsen. Sự nghiệp chính trị cũng như tiền đồ của nhà Woodsen sụp đổ khi John Woodsen bị cáo buộc góp phần chính vào thảm hoạ tại Rambabos. Trước khi ứng cử cho vị trí thống đốc, ông Woodsen từng ở vị trí chỉ huy của A.A.A. Lệnh ngừng bắn quá muộn màng của ông đã khiến cho các đối thủ chính trị lấy đây làm cớ để hạ bệ ông. John Woodsen không chỉ bị tước đi vị trí, các danh hiệu mà còn bị bắt giam. Ethan vốn là một chàng trai ham học, yêu sách vở và tốt bụng. Năm 18 tuổi, cậu từ bỏ trường đại học và đăng ký làm ứng viên cho A.A.A vì muốn cứu vãn bộ mặt của dòng họ danh giá Van der Woodsen sau sự việc đáng tiếc của người cha. Ethan tự mang trong mình trọng trách điều tra vụ việc năm xưa để giải oan cho cha và cho cả loài người. Khi mới bắt đầu quá trình luyện tập, nhiều đồng đội coi thường Ethan bởi vẻ ngoài yếu ớt công tử của cậu. Trải qua 3 năm tập huấn cực nhọc, không ai có thể tin rằng chàng trai mảnh khảnh này có thể vượt qua được bài kiểm tra khắc nhiệt của A.A.A. Ethan biến điểm yếu của mình thành lợi thế, cậu luôn nhanh nhạy hơn các đối thủ cả về đầu óc lẫn thể chất. Khi trở thành một astroguard, Ethan lấy tên nhiệm vụ là Cosmo. Trong ngày đầu làm nhiệm vụ, Cosmo tìm thấy chiếc tàu vũ trụ thất lạc có Zina đã bị băng hoá ở bên trong. Anh giúp cô sống lại nhưng cô hoàn toàn quên mất mình là ai, thuộc hành tinh nào. Cosmo tốt bụng, quyết định giúp đỡ cô gái xa lạ tìm đường về nhà.", true, false, "Cosmo", 100L, null, null, null },
                    { new Guid("fa9289ea-0502-4b01-8f54-ca742a95d048"), "ZINA", new DateTime(2024, 1, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Zina – công chúa của Zaeperi là một cô gái xinh đẹp và quả cảm. Trong trận Rambabos, cả gia đình hoàng gia bị những kẻ lạ mặt bắt đi nhưng Zina tìm cách trốn thoát. Cô trốn vào khoang băng của một con tàu vũ trụ không người lái và mắc kẹt luôn ở đó. Sau này, Zina được Cosmo tìm thấy nhưng cô đã mất một phần trí nhớ. Cosmo muốn giúp cô tìm lại hành tinh của mình. Họ bước vào hành trình đi qua 27 hành tinh để tìm kiếm dữ liệu, đầu mối. Zina học tiếng Anh và văn hoá của loài người dưới sự hướng dẫn đầy kiên nhẫn của Cosmo. Quá trình học tập chăm chỉ khiến các chức năng não tốt lên, Zina dần khôi phục lại các ký ức. Một ngày, khi các ký ức khôi phục hoàn toàn, Zina nhận ra Cosmo – vị ân nhân cứu cô thuộc về đội quân đã huỷ diệt quê hương, cô đối mặt với sự đấu tranh tâm lý nặng nề. Cosmo cùng với lòng tốt và sự thông minh đã chứng minh sự vô tội của loài người và thuyết phục Zina đứng về phía mình. Zina có một người em song sinh là Zinie. Zinie có phần cá tính, phá phách hơn. Bằng cách nào đó, Zinie cũng thoát khỏi thế lực bí ẩn kia và cố gắng tìm chị gái. Khi biết Zina đi cùng Cosmo, cô bé nghĩ rằng loài người đã tẩy não và bắt cóc chị mình. Zinie từng bước lập kế hoạch phá tàu Atlantic của Cosmo để giải cứu chị.", true, false, "Zina", 100L, null, null, null }
                });

            migrationBuilder.UpdateData(
                table: "SpaceShips",
                keyColumn: "Id",
                keyValue: new Guid("46be8251-f95a-4e1b-b451-2a3fe2b4a5bc"),
                columns: new[] { "Description", "Price" },
                values: new object[] { "Thiên hoa hạ phàm: Triệu hồi 1 cầu thiên thạch giữa màn hình, animation hút không khí xung quanh trong 3 giây, rồi tỏa ra luồng khí băng bao phủ toàn bộ sân đấu (Thiên thạch sẽ xoay vòng và tỏa ra thành hình bông hoa sen băng nở rộ). Tất cả thiên thạch sẽ giảm bị đóng băng và đứng yên trong 1s", 100L });

            migrationBuilder.InsertData(
                table: "SpaceShips",
                columns: new[] { "Id", "Code", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "Description", "IsDefault", "IsDeleted", "Name", "Price", "SpaceShipId", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[,]
                {
                    { new Guid("0e92e6cb-6d08-48a6-9c08-740d38ed994e"), "KINGHT", new DateTime(2024, 1, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Triệu hồi 1 thanh hồn kiếm xuống giữa màn hình, tỏa sóng xung kính ra xung quanh, tất cả thiên thạch sẽ bị giảm tốc độ 2s mỗi thiên thạch (Thời gian : 10s, hồi 100point)", false, false, "Kinght", 100L, null, null, null, null },
                    { new Guid("2e056340-c4a6-4fdf-9200-13b096322afc"), "SIGMA", new DateTime(2021, 1, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Terrabrain Synchronization (Đồng bộ hóa địa hình): tạo ra 1 cổng không gian ngay trước phi thuyền, các thiên thạch đi qua lập tức reset thời gian thiên thạch về tối đa - thiên thạch chuyển vị trí rơi lại từ đầu", false, false, "Sigma", 100L, null, null, null, null },
                    { new Guid("4c09ef0c-58de-4ab6-a02a-bafaef2c8fb9"), "POSEIDON", new DateTime(2021, 1, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Triệu hồi đinh ba xuất hiện phía trên con thuyền, từ con thuyền lên nửa màn hình sẽ tạo ra các đợt sóng đâm thẳng lên ( 3 đợt trong 10s), mỗi đợt đi qua 1 thiên thạch sẽ khiến nó bị đẩy lùi ( thời gian rơi + 2s)", false, false, "Poseidon", 100L, null, null, null, null },
                    { new Guid("bb1e3626-3292-4e46-943a-a9f853a19f37"), "WINDY", new DateTime(2021, 1, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Lãnh thổ Bồ Công Anh: Triệu hồi gió kèm các bông hoa bồ công anh mờ ảo bay lên, cuốn các thiên thạch vào trong 1 vùng tròn ở giữa (ưu tiên hút các thiên thạch gần nhất người dùng), hút tối đa 3 thiên thạch, sau 10s các thiên thạch sẽ tách nhau ra và rơi xuống tiếp, trong thời gian này nếu trả lời đúng 1 câu hỏi (2 với vòng 10 trở đi) trong 3 thiên thạch lập tức phá hủy cả 3 thiên thạch và lập tức tắt lãnh thổ", false, false, "Windy", 100L, null, null, null, null },
                    { new Guid("c83475cb-9980-4bfb-8fe8-2de32512ed88"), "HORIZON", new DateTime(2021, 1, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Heaven's Wrath: Triệu hồi 1 cơn bão sấm sét, trong vòng 10s sẽ đánh xuống các địa điểm bất kỳ , các địa điểm này sau đó sẽ tồn tại 1 cục bóng điện, các thiên thạch đi qua sẽ bị giật và đứng yên trong 3s ( Địa điểm sét đánh tối đa: 4 - cứ 2.5s sẽ đánh xuống 1 tia sét - thiên thạch đi qua sẽ lập tức biến mất)", false, false, "Horizon", 100L, null, null, null, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_GameAnswers_ZMatterId",
                table: "GameAnswers",
                column: "ZMatterId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentCharacters_CharacterId",
                table: "StudentCharacters",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentCharacters_StudentGameInfoId",
                table: "StudentCharacters",
                column: "StudentGameInfoId");

            migrationBuilder.AddForeignKey(
                name: "FK_GameAnswers_ZMatters_ZMatterId",
                table: "GameAnswers",
                column: "ZMatterId",
                principalTable: "ZMatters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GameAnswers_ZMatters_ZMatterId",
                table: "GameAnswers");

            migrationBuilder.DropTable(
                name: "StudentCharacters");

            migrationBuilder.DropTable(
                name: "Characters");

            migrationBuilder.DropIndex(
                name: "IX_GameAnswers_ZMatterId",
                table: "GameAnswers");

            migrationBuilder.DeleteData(
                table: "SpaceShips",
                keyColumn: "Id",
                keyValue: new Guid("0e92e6cb-6d08-48a6-9c08-740d38ed994e"));

            migrationBuilder.DeleteData(
                table: "SpaceShips",
                keyColumn: "Id",
                keyValue: new Guid("2e056340-c4a6-4fdf-9200-13b096322afc"));

            migrationBuilder.DeleteData(
                table: "SpaceShips",
                keyColumn: "Id",
                keyValue: new Guid("4c09ef0c-58de-4ab6-a02a-bafaef2c8fb9"));

            migrationBuilder.DeleteData(
                table: "SpaceShips",
                keyColumn: "Id",
                keyValue: new Guid("bb1e3626-3292-4e46-943a-a9f853a19f37"));

            migrationBuilder.DeleteData(
                table: "SpaceShips",
                keyColumn: "Id",
                keyValue: new Guid("c83475cb-9980-4bfb-8fe8-2de32512ed88"));

            migrationBuilder.DropColumn(
                name: "Description",
                table: "SpaceShips");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "SpaceShips");

            migrationBuilder.DropColumn(
                name: "ZMatterId",
                table: "GameAnswers");

            migrationBuilder.AddColumn<string>(
                name: "Gender",
                table: "StudentGameInfos",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }
    }
}
