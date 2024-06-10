using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Fsel.System.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_QuestboardTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuestBoardConfigs");

            migrationBuilder.DropColumn(
                name: "AchievedPoints",
                table: "QuestBoardStudents");

            migrationBuilder.DropColumn(
                name: "CourseId",
                table: "QuestBoardStudents");

            migrationBuilder.DropColumn(
                name: "DependentId",
                table: "QuestBoards");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "QuestBoards");

            migrationBuilder.DropColumn(
                name: "IsLifeTime",
                table: "QuestBoards");

            migrationBuilder.DropColumn(
                name: "IsRequired",
                table: "QuestBoards");

            migrationBuilder.DropColumn(
                name: "PackageIdsStr",
                table: "QuestBoards");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "QuestBoards");

            migrationBuilder.RenameColumn(
                name: "ObjectId",
                table: "QuestBoardStudents",
                newName: "QuestBoardOverallStudentId");

            migrationBuilder.RenameColumn(
                name: "NumberOfStars",
                table: "QuestBoards",
                newName: "Token");

            migrationBuilder.AddColumn<int>(
                name: "CurrentValue",
                table: "QuestBoardStudents",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Energy",
                table: "QuestBoardStudents",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Token",
                table: "QuestBoardStudents",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Energy",
                table: "QuestBoards",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TargetValue",
                table: "QuestBoards",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "QuestBoardOveralls",
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
                    Type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TargetValue = table.Column<int>(type: "int", nullable: false),
                    Token = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestBoardOveralls", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QuestBoardOverallStudents",
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
                    QuestBoardOverallId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CurrentValue = table.Column<int>(type: "int", nullable: false),
                    Token = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestBoardOverallStudents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuestBoardOverallStudents_QuestBoardOveralls_QuestBoardOverallId",
                        column: x => x.QuestBoardOverallId,
                        principalTable: "QuestBoardOveralls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "QuestBoardOveralls",
                columns: new[] { "Id", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "IsDeleted", "TargetValue", "Token", "Type", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[,]
                {
                    { new Guid("47742457-c637-4244-a8f4-32e8820acd40"), new DateTime(2024, 5, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, 4, 20, "BeginnerQuests", null, null, null },
                    { new Guid("59c4f992-cc12-4cfc-baf8-4929de320bc2"), new DateTime(2024, 5, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, 6, 40, "BeginnerQuests", null, null, null },
                    { new Guid("8f32d29f-1787-42fe-9f0d-e5e40eab7e31"), new DateTime(2024, 5, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, 2, 10, "BeginnerQuests", null, null, null },
                    { new Guid("fead115f-827a-4468-b8a2-9ded08323a5e"), new DateTime(2024, 5, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, false, 140, 100, "LearningQuests", null, null, null }
                });

            migrationBuilder.InsertData(
                table: "QuestBoards",
                columns: new[] { "Id", "Category", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "Description", "Energy", "ImagePath", "IsActive", "IsDeleted", "Name", "RepeatType", "TargetValue", "Token", "Type", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[,]
                {
                    { new Guid("0ac5668a-1d21-4c05-96b6-94012d183ae6"), "LearningSpaceship", new DateTime(2024, 5, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Thời gian truy cập tính năng 'Học' đạt 120 phút", 20, "", true, false, "Phi Thuyền Học Tập", "Week", 1, 5, "LearningQuests", null, null, null },
                    { new Guid("1483c74d-d798-46e7-b152-51afd03ff88e"), "CompleteTheFirstClassForum", new DateTime(2024, 5, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Hoàn thành Diễn đàn lớp học đầu tiên", null, "", true, false, "Tân thủ III", null, 1, 20, "BeginnerQuests", null, null, null },
                    { new Guid("1a467647-049c-47c7-a596-8ff5bf33ff10"), "CompleteMissionDay", new DateTime(2024, 5, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Hoàn thành mốc tập trung của hôm nay", 2, "", true, false, "Hoàn Thành Sứ Mệnh Ngày", "Day", 1, 2, "LearningQuests", null, null, null },
                    { new Guid("2b4498eb-e566-438b-8c0d-2f8ac240a400"), "CompleteTheFirstTest", new DateTime(2024, 5, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Hoàn thành bài kiểm tra đầu tiên của bạn", null, "", true, false, "Tân thủ I", null, 1, 20, "BeginnerQuests", null, null, null },
                    { new Guid("393dcd7e-9b4b-4a69-a222-6981c3cc1967"), "HistoryOfDiscovery", new DateTime(2024, 5, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Xem lại một Video bài học đã hoàn thành", 2, "", true, false, "Lịch sử khám phá", "Day", 1, 2, "LearningQuests", null, null, null },
                    { new Guid("418e49eb-54ff-4e8a-b87a-368b9428d74b"), "ConqueringAsteroids", new DateTime(2024, 5, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Hoàn thành 1 bài tập về nhà với điểm tổng kết trên 50%", 2, "", true, false, "Chinh Phục Tiểu Hành Tinh", "Day", 1, 2, "LearningQuests", null, null, null },
                    { new Guid("540839db-2511-4074-990f-d69a3b75fb90"), "ConnectingAllies", new DateTime(2024, 5, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Đăng tải bình luận trên bài viết thuộc Diễn đàn lớp học gần nhất", 2, "", true, false, "Kết Nối Đồng Minh", "Day", 1, 2, "LearningQuests", null, null, null },
                    { new Guid("6cee1462-2590-420f-bd50-d47981f88a21"), "CompleteTheFirstVideoLesson", new DateTime(2024, 5, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Hoàn thành Video bài học đầu tiên", null, "", true, false, "Tân thủ II", null, 1, 20, "BeginnerQuests", null, null, null },
                    { new Guid("73421144-de83-4b9f-9eea-d0b2c8dc6530"), "ExploreTheLearningGalaxy", new DateTime(2024, 5, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Tương tác tính năng 'Học' trong 20 phút", 2, "", true, false, "Khám Phá Thiên Hà Học Tập", "Day", 20, 2, "LearningQuests", null, null, null },
                    { new Guid("7372c5a5-4e53-4da4-a6f2-7c992abb014a"), "BackupNotes", new DateTime(2024, 5, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Xem lại nôi dung bạn đã ghi chú", 2, "", true, false, "Sao Lưu Ghi Chú", "Day", 1, 2, "LearningQuests", null, null, null },
                    { new Guid("8cc59caa-05f4-4735-9787-9d1e3cbd8864"), "CompleteHomeworkFirst", new DateTime(2024, 5, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Hoàn thành Bài tập về nhà đầu tiên", null, "", true, false, "Tân thủ IV", null, 1, 20, "BeginnerQuests", null, null, null },
                    { new Guid("9069f184-fd45-413e-b56a-79852879282e"), "CompleteFocusModeFirst", new DateTime(2024, 5, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Hoàn thành Focus Mode đầu tiên", null, "", true, false, "Tân thủ V", null, 1, 20, "BeginnerQuests", null, null, null },
                    { new Guid("909cee82-d102-4eaa-8f51-519f4e4740f1"), "InfinityFocusMode", new DateTime(2024, 5, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Hoàn thành 7 lần Focus Mode", 20, "", true, false, "Chế Độ Tập Trung Vô Cực", "Week", 1, 5, "LearningQuests", null, null, null },
                    { new Guid("91ef56a9-3638-4994-b136-019c9ec51120"), "DecodingTheNebula", new DateTime(2024, 5, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Hoàn thành một câu hỏi trong Video bài học", 2, "", true, false, "Giải Mã Tinh Vân", "Day", 1, 2, "LearningQuests", null, null, null },
                    { new Guid("932ed712-dc2b-42dd-a700-de944018ad0e"), "GalaxyNotes", new DateTime(2024, 5, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Tạo một ghi chú mới", 2, "", true, false, "Ghi Chú Thiên Hà", "Day", 1, 2, "LearningQuests", null, null, null },
                    { new Guid("9f57609b-1f81-4a42-a104-5aa2dca21b2e"), "InterstellarInteractions", new DateTime(2024, 5, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Nhận xét hoặc Thích một bài viết trong Diễn đàn chung", 2, "", true, false, "Tương tác liên sao", "Day", 1, 2, "LearningQuests", null, null, null },
                    { new Guid("b6be6e8b-3317-4f25-96b7-8ad1fa151793"), "MessagesFromAI", new DateTime(2024, 5, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Xem phản hồi của AI cho bài đăng Class Forum của bạn", 20, "", true, false, "Thông Điệp Từ AI", "Week", 20, 5, "LearningQuests", null, null, null },
                    { new Guid("c77a4195-ee46-4639-990e-6aa6e25284fa"), "CompleteTheFirstUnit", new DateTime(2024, 5, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Hoàn thành Unit đầu tiên", null, "", true, false, "Tân thủ VI", null, 1, 20, "BeginnerQuests", null, null, null },
                    { new Guid("d33f9400-adf9-431c-b97c-e679321f4f17"), "JourneyOfKnowledge", new DateTime(2024, 5, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Hoàn thành 20 câu hỏi trong Video bài học", 20, "", true, false, "Hành Trình Tri Thức", "Week", 120, 5, "LearningQuests", null, null, null },
                    { new Guid("e6fc1b44-57aa-44bc-989f-5e34d4152b9d"), "TheMysteryOfTheStars", new DateTime(2024, 5, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Hoàn thành 10 bài tập về nhà", 20, "", true, false, "Bí Ẩn Của Vì Sao", "Week", 10, 5, "LearningQuests", null, null, null },
                    { new Guid("eec844f3-2574-4a0d-a6a6-6b12f07264dd"), "SharedRocketLaunch", new DateTime(2024, 5, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Tạo một bài đăng trên Diễn đàn chung", 2, "", true, false, "Phóng Tên Lửa Chia Sẻ", "Day", 1, 2, "LearningQuests", null, null, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuestBoardStudents_QuestBoardOverallStudentId",
                table: "QuestBoardStudents",
                column: "QuestBoardOverallStudentId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestBoardOverallStudents_QuestBoardOverallId",
                table: "QuestBoardOverallStudents",
                column: "QuestBoardOverallId");

            migrationBuilder.AddForeignKey(
                name: "FK_QuestBoardStudents_QuestBoardOverallStudents_QuestBoardOverallStudentId",
                table: "QuestBoardStudents",
                column: "QuestBoardOverallStudentId",
                principalTable: "QuestBoardOverallStudents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuestBoardStudents_QuestBoardOverallStudents_QuestBoardOverallStudentId",
                table: "QuestBoardStudents");

            migrationBuilder.DropTable(
                name: "QuestBoardOverallStudents");

            migrationBuilder.DropTable(
                name: "QuestBoardOveralls");

            migrationBuilder.DropIndex(
                name: "IX_QuestBoardStudents_QuestBoardOverallStudentId",
                table: "QuestBoardStudents");

            migrationBuilder.DeleteData(
                table: "QuestBoards",
                keyColumn: "Id",
                keyValue: new Guid("0ac5668a-1d21-4c05-96b6-94012d183ae6"));

            migrationBuilder.DeleteData(
                table: "QuestBoards",
                keyColumn: "Id",
                keyValue: new Guid("1483c74d-d798-46e7-b152-51afd03ff88e"));

            migrationBuilder.DeleteData(
                table: "QuestBoards",
                keyColumn: "Id",
                keyValue: new Guid("1a467647-049c-47c7-a596-8ff5bf33ff10"));

            migrationBuilder.DeleteData(
                table: "QuestBoards",
                keyColumn: "Id",
                keyValue: new Guid("2b4498eb-e566-438b-8c0d-2f8ac240a400"));

            migrationBuilder.DeleteData(
                table: "QuestBoards",
                keyColumn: "Id",
                keyValue: new Guid("393dcd7e-9b4b-4a69-a222-6981c3cc1967"));

            migrationBuilder.DeleteData(
                table: "QuestBoards",
                keyColumn: "Id",
                keyValue: new Guid("418e49eb-54ff-4e8a-b87a-368b9428d74b"));

            migrationBuilder.DeleteData(
                table: "QuestBoards",
                keyColumn: "Id",
                keyValue: new Guid("540839db-2511-4074-990f-d69a3b75fb90"));

            migrationBuilder.DeleteData(
                table: "QuestBoards",
                keyColumn: "Id",
                keyValue: new Guid("6cee1462-2590-420f-bd50-d47981f88a21"));

            migrationBuilder.DeleteData(
                table: "QuestBoards",
                keyColumn: "Id",
                keyValue: new Guid("73421144-de83-4b9f-9eea-d0b2c8dc6530"));

            migrationBuilder.DeleteData(
                table: "QuestBoards",
                keyColumn: "Id",
                keyValue: new Guid("7372c5a5-4e53-4da4-a6f2-7c992abb014a"));

            migrationBuilder.DeleteData(
                table: "QuestBoards",
                keyColumn: "Id",
                keyValue: new Guid("8cc59caa-05f4-4735-9787-9d1e3cbd8864"));

            migrationBuilder.DeleteData(
                table: "QuestBoards",
                keyColumn: "Id",
                keyValue: new Guid("9069f184-fd45-413e-b56a-79852879282e"));

            migrationBuilder.DeleteData(
                table: "QuestBoards",
                keyColumn: "Id",
                keyValue: new Guid("909cee82-d102-4eaa-8f51-519f4e4740f1"));

            migrationBuilder.DeleteData(
                table: "QuestBoards",
                keyColumn: "Id",
                keyValue: new Guid("91ef56a9-3638-4994-b136-019c9ec51120"));

            migrationBuilder.DeleteData(
                table: "QuestBoards",
                keyColumn: "Id",
                keyValue: new Guid("932ed712-dc2b-42dd-a700-de944018ad0e"));

            migrationBuilder.DeleteData(
                table: "QuestBoards",
                keyColumn: "Id",
                keyValue: new Guid("9f57609b-1f81-4a42-a104-5aa2dca21b2e"));

            migrationBuilder.DeleteData(
                table: "QuestBoards",
                keyColumn: "Id",
                keyValue: new Guid("b6be6e8b-3317-4f25-96b7-8ad1fa151793"));

            migrationBuilder.DeleteData(
                table: "QuestBoards",
                keyColumn: "Id",
                keyValue: new Guid("c77a4195-ee46-4639-990e-6aa6e25284fa"));

            migrationBuilder.DeleteData(
                table: "QuestBoards",
                keyColumn: "Id",
                keyValue: new Guid("d33f9400-adf9-431c-b97c-e679321f4f17"));

            migrationBuilder.DeleteData(
                table: "QuestBoards",
                keyColumn: "Id",
                keyValue: new Guid("e6fc1b44-57aa-44bc-989f-5e34d4152b9d"));

            migrationBuilder.DeleteData(
                table: "QuestBoards",
                keyColumn: "Id",
                keyValue: new Guid("eec844f3-2574-4a0d-a6a6-6b12f07264dd"));

            migrationBuilder.DropColumn(
                name: "CurrentValue",
                table: "QuestBoardStudents");

            migrationBuilder.DropColumn(
                name: "Energy",
                table: "QuestBoardStudents");

            migrationBuilder.DropColumn(
                name: "Token",
                table: "QuestBoardStudents");

            migrationBuilder.DropColumn(
                name: "Energy",
                table: "QuestBoards");

            migrationBuilder.DropColumn(
                name: "TargetValue",
                table: "QuestBoards");

            migrationBuilder.RenameColumn(
                name: "QuestBoardOverallStudentId",
                table: "QuestBoardStudents",
                newName: "ObjectId");

            migrationBuilder.RenameColumn(
                name: "Token",
                table: "QuestBoards",
                newName: "NumberOfStars");

            migrationBuilder.AddColumn<float>(
                name: "AchievedPoints",
                table: "QuestBoardStudents",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<Guid>(
                name: "CourseId",
                table: "QuestBoardStudents",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "DependentId",
                table: "QuestBoards",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "QuestBoards",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsLifeTime",
                table: "QuestBoards",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsRequired",
                table: "QuestBoards",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PackageIdsStr",
                table: "QuestBoards",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "QuestBoards",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "QuestBoardConfigs",
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
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DisplayType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MaxPoints = table.Column<int>(type: "int", nullable: false),
                    Operator = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TaskPageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestBoardConfigs", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "QuestBoardConfigs",
                columns: new[] { "Id", "Category", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "DisplayType", "IsDeleted", "MaxPoints", "Operator", "TaskPageUrl", "Type", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[,]
                {
                    { new Guid("0407d174-a779-46e9-bbe4-5f27ae6075b6"), "ParticipationScore", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Percent", false, 80, "GreaterThan", null, "PremiumQuests", null, null, null },
                    { new Guid("0417a848-1cd0-4aef-ae33-2757652701d0"), "RateAndComment", new DateTime(2023, 11, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Number", false, 1, "Equal", null, "SideQuests", null, null, null },
                    { new Guid("0aee60ff-65bc-4277-803c-8a21c38b2b84"), "ThirtyMinutesFocusMode", new DateTime(2023, 11, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Number", false, 1, "Equal", null, "SideQuests", null, null, null },
                    { new Guid("0c1633d5-f145-44c7-80c3-a167c0bbd1a1"), "PostThreeDiscussionBoard", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Number", false, 3, "Equal", null, "PremiumQuests", null, null, null },
                    { new Guid("1b35cdbf-98a7-4ce7-9b71-f457b386d62c"), "FinishOneClassForumPost", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Number", false, 1, "Equal", null, "MainQuests", null, null, null },
                    { new Guid("2ea59046-ea24-4f3f-b390-36d9629ae11e"), "CommentOnNewLessonOfTwoClassMate", new DateTime(2023, 11, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Number", false, 2, "Equal", null, "DailyQuests", null, null, null },
                    { new Guid("31c2d3aa-ec1d-4ebb-99d1-5a8a2ae391c7"), "LearnInteractTwentyMinutes", new DateTime(2023, 11, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Number", false, 1, "Equal", null, "DailyQuests", null, null, null },
                    { new Guid("38045664-20f8-4780-9c62-9736c2bce90c"), "FinishDailyFocusMode", new DateTime(2023, 11, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Number", false, 1, "Equal", null, "DailyQuests", null, null, null },
                    { new Guid("64c0a3a5-849c-412d-86b6-3f5e4809cc84"), "FinishOneFinalTest", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Number", false, 1, "Equal", null, "MainQuests", null, null, null },
                    { new Guid("68bd35d3-c09b-45f3-9a05-0e10691f7c42"), "FinishOneLesson", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Number", false, 1, "Equal", null, "MainQuests", null, null, null },
                    { new Guid("72933280-e14b-4715-a802-dcd88e031e79"), "FinishOneHomeworkMiniProject", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Number", false, 1, "Equal", null, "MainQuests", null, null, null },
                    { new Guid("7a37fa61-1a04-4d70-8590-90fa3d563d8e"), "SeeAllTeacherReview", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Percent", false, 100, "Equal", null, "PremiumQuests", null, null, null },
                    { new Guid("7b436441-ef2f-4a83-be79-ee5ec5f18355"), "FinishOneUnit", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Number", false, 1, "Equal", null, "MainQuests", null, null, null },
                    { new Guid("8333c5e5-e853-45fb-8abf-1a09336af78e"), "SeeFiveTeacherReview", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Number", false, 5, "Equal", null, "PremiumQuests", null, null, null },
                    { new Guid("85659470-7d45-4ddf-8b3c-dba6458bf4f4"), "PostFiveDiscussionBoard", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Number", false, 5, "Equal", null, "PremiumQuests", null, null, null },
                    { new Guid("9935cdbf-98a7-4ce7-9b71-f457b386d64c"), "CommentOnOtherPost", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Number", false, 1, "Equal", null, "MainQuests", null, null, null },
                    { new Guid("9d333771-8c0d-481c-95e7-542a12002684"), "CompleteHomeWorkAtLeastFiftyPercent", new DateTime(2023, 11, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Number", false, 1, "Equal", null, "DailyQuests", null, null, null },
                    { new Guid("ac0f0c74-cc2a-41a2-a82f-748ed5f2c75c"), "SeeTenTeacherReview", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Number", false, 10, "Equal", null, "PremiumQuests", null, null, null },
                    { new Guid("bb35cdbf-98a7-4ce7-9b71-f457b386d64c"), "FinishOneLevelPass", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Number", false, 1, "Equal", null, "MainQuests", null, null, null },
                    { new Guid("c3c2c8aa-fc4a-4b01-8d55-1026062a47c6"), "SuccessfulIntroduceCode", new DateTime(2023, 11, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Number", false, 1, "Equal", null, "SideQuests", null, null, null },
                    { new Guid("cfce3f4b-66d5-467d-8f57-50a089257bcb"), "FinishOneLesson", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Number", false, 1, "Equal", null, "MainQuests", null, null, null },
                    { new Guid("d34cf82a-8582-4dd8-b76b-e89857f4c910"), "NinetyMinutesFocusMode", new DateTime(2023, 11, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Number", false, 1, "Equal", null, "SideQuests", null, null, null },
                    { new Guid("e0608ce3-6514-4fd2-88f6-f62db89e5fa7"), "OneHundredTwentytyMinutesFocusMode", new DateTime(2023, 11, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Number", false, 1, "Equal", null, "SideQuests", null, null, null },
                    { new Guid("e527e048-f02e-4a22-bec4-427b67c93d63"), "OneHundredEightyMinutesFocusMode", new DateTime(2023, 11, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Number", false, 1, "Equal", null, "SideQuests", null, null, null },
                    { new Guid("f31d44ff-226e-4244-a9a0-a06ed397368a"), "PostOneDiscussionBoard", new DateTime(2023, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Number", false, 1, "Equal", null, "PremiumQuests", null, null, null },
                    { new Guid("f62373aa-3fde-4848-8fef-7567fd0c7e8b"), "SixtyMinutesFocusMode", new DateTime(2023, 11, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Number", false, 1, "Equal", null, "SideQuests", null, null, null },
                    { new Guid("f8c5416c-118e-45dc-942c-f61e985b9827"), "ReviseYourNotes", new DateTime(2023, 11, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "Number", false, 1, "Equal", null, "DailyQuests", null, null, null }
                });
        }
    }
}
