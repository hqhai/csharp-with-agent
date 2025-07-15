using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Fsel.Identity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_PermissionEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PermissionGroupId",
                table: "AspNetRoleClaims",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "PermissionId",
                table: "AspNetRoleClaims",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<bool>(
                name: "Status",
                table: "AspNetRoleClaims",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "PermissionGroups",
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
                    ClaimType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Status = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermissionGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Permissions",
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
                    ClaimValue = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Status = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    PermissionGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Permissions_PermissionGroups_PermissionGroupId",
                        column: x => x.PermissionGroupId,
                        principalTable: "PermissionGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "PermissionGroups",
                columns: new[] { "Id", "ClaimType", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "Description", "IsDeleted", "Name", "Status", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[,]
                {
                    { new Guid("22960350-3a0f-468b-849f-bf0fd7db264b"), "PopupOrderManagement", new DateTime(2025, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Quản lý thứ tự popup", true, null, null, null },
                    { new Guid("31bafbd0-7346-4aaf-a1fa-bf2194b1be68"), "RoleGroupManagement", new DateTime(2025, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Quản lý nhóm quyền", true, null, null, null },
                    { new Guid("39d8915b-4373-4ed2-9a79-d59db3a5285f"), "StudentProgressManagement", new DateTime(2025, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Quản lý tiến độ học sinh", true, null, null, null },
                    { new Guid("3d33a7c7-2cf2-4383-9487-e88c0640d12c"), "GiftManagement", new DateTime(2025, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Quản lý quà tặng", true, null, null, null },
                    { new Guid("57cbd9f0-87d5-4c5c-8986-08e6fae58d99"), "VoucherManagement", new DateTime(2025, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Quản lý voucher", true, null, null, null },
                    { new Guid("5f09fd6f-3a0b-4c40-85b6-0d5cbd85c3ce"), "UserGroupManagement", new DateTime(2025, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Quản lý nhóm người dùng", true, null, null, null },
                    { new Guid("68e5e3d2-90a1-4c60-9b8f-77a03381dc92"), "StudentManagement", new DateTime(2025, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Quản lý học sinh", true, null, null, null },
                    { new Guid("6f12e0d0-774c-4868-a062-e94b0b6ecf2d"), "AddStudentToBlindBox", new DateTime(2025, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Thêm học sinh vào túi mù", true, null, null, null },
                    { new Guid("89b8e2df-df40-4cb6-9f86-7a7fa315a194"), "HomeDashboard", new DateTime(2025, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Home/Dashboard", true, null, null, null },
                    { new Guid("95a2cf57-3b18-41b4-b5d5-9291a7bb87a5"), "TaskManagement", new DateTime(2025, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Quản lý nhiệm vụ", true, null, null, null },
                    { new Guid("9fc99947-c7a9-44aa-b9eb-ea42b28590fc"), "PermissionManagement", new DateTime(2025, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Quản lý quyền", true, null, null, null },
                    { new Guid("a1a7a7be-8865-49b1-a0be-bb573b888fd3"), "ReportManagement", new DateTime(2025, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Quản lý báo cáo", true, null, null, null },
                    { new Guid("ab4582fd-e613-4904-bdc3-c50ff03a1d4d"), "ErrorReportManagement", new DateTime(2025, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Quản lý báo cáo lỗi", true, null, null, null },
                    { new Guid("b0a0b3b1-35aa-4ef5-8c17-95a5b7cf7a2b"), "ClassManagement", new DateTime(2025, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Quản lý lớp", true, null, null, null },
                    { new Guid("b2b1272c-3b9d-4c6e-91f4-60c302c7d7e1"), "PromotionManagement", new DateTime(2025, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Quản lý ưu đãi", true, null, null, null },
                    { new Guid("b369b45f-2b8e-4cb0-91c2-2ad73d29d88d"), "PaymentManagement", new DateTime(2025, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Quản lý thanh toán", true, null, null, null },
                    { new Guid("b652d6d2-f5f0-4c5d-a8b1-ec58860cc6d7"), "ReferralCodeManagement", new DateTime(2025, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Quản lý mã giới thiệu", true, null, null, null },
                    { new Guid("c9c55ef5-01e0-4fa6-b68a-3aaea1089548"), "UserManagement", new DateTime(2025, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Quản lý người dùng", true, null, null, null },
                    { new Guid("d9679732-17a3-44ff-b4ed-b39f631c3e13"), "BannerManagement", new DateTime(2025, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Quản lý banner", true, null, null, null },
                    { new Guid("df96b682-99c5-4ae2-9516-329d2f2d3054"), "CourseGoalManagement", new DateTime(2025, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Quản lý mục tiêu khóa học", true, null, null, null },
                    { new Guid("e4961eb7-931a-4403-b94a-fb2f7860b80e"), "PriceManagement", new DateTime(2025, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Quản lý bảng giá", true, null, null, null },
                    { new Guid("e758b51b-74de-4a2b-87b3-394f701bce9f"), "ForbiddenWordsStorage", new DateTime(2025, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Kho từ cấm", true, null, null, null },
                    { new Guid("f9142961-5fe0-4015-8d01-9e12d202a7ae"), "AuthorizationManagement", new DateTime(2025, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Quản lý phân quyền", true, null, null, null }
                });

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "ClaimValue", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "Description", "IsDeleted", "Name", "PermissionGroupId", "Status", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[,]
                {
                    { new Guid("07e2099e-4c7a-4c93-bbb6-3cf9ff9fcfa7"), "PermissionManagement.Delete", new DateTime(2025, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Xóa quyền", new Guid("9fc99947-c7a9-44aa-b9eb-ea42b28590fc"), true, null, null, null },
                    { new Guid("0f2aeb4f-4e34-4375-9757-b546fb4aa4c7"), "ErrorReportManagement.View", new DateTime(2025, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Xem thông tin danh sách lỗi", new Guid("ab4582fd-e613-4904-bdc3-c50ff03a1d4d"), true, null, null, null },
                    { new Guid("15471d32-9b2f-489e-b23e-6ec1a476973f"), "BannerManagement.Update", new DateTime(2025, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Chỉnh sửa/bật/tắt/xóa banner", new Guid("d9679732-17a3-44ff-b4ed-b39f631c3e13"), true, null, null, null },
                    { new Guid("19b8e2df-d240-4cb6-9f86-7a7fa315a186"), "AuthorizationManagement.Update", new DateTime(2025, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Chỉnh sửa phân quyền", new Guid("f9142961-5fe0-4015-8d01-9e12d202a7ae"), true, null, null, null },
                    { new Guid("1a1ab73e-7fcb-4d49-a4ff-f3c9b299f58d"), "GiftManagement.Add", new DateTime(2025, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Thêm mới thông tin quà tặng", new Guid("3d33a7c7-2cf2-4383-9487-e88c0640d12c"), true, null, null, null },
                    { new Guid("1a3d195c-6b7f-46a6-bd3b-c43e4e276bd2"), "ForbiddenWordsStorage.Update", new DateTime(2025, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Xóa/chỉnh sửa thông tin từ cấm", new Guid("e758b51b-74de-4a2b-87b3-394f701bce9f"), true, null, null, null },
                    { new Guid("1ad1231e-7994-4659-b872-90e4f43794f0"), "ClassManagement.Add", new DateTime(2025, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Thêm mới lớp", new Guid("b0a0b3b1-35aa-4ef5-8c17-95a5b7cf7a2b"), true, null, null, null },
                    { new Guid("1e6ac8a6-d1d4-48ed-86c3-8856d1086172"), "UserGroupManagement.Update", new DateTime(2025, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Chỉnh sửa nhóm người dùng", new Guid("5f09fd6f-3a0b-4c40-85b6-0d5cbd85c3ce"), true, null, null, null },
                    { new Guid("1fae41db-67cf-4b6f-8dc8-c01be7fceae4"), "StudentManagement.Update", new DateTime(2025, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Chỉnh sửa thông tin học sinh", new Guid("68e5e3d2-90a1-4c60-9b8f-77a03381dc92"), true, null, null, null },
                    { new Guid("23b5e2df-df40-4cb6-9f86-9a7fa215a686"), "AddStudentToBlindBox.Add", new DateTime(2025, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Thêm học sinh vào túi mù", new Guid("6f12e0d0-774c-4868-a062-e94b0b6ecf2d"), true, null, null, null },
                    { new Guid("26fae26a-1994-4328-a4f2-94b8a36c8c2d"), "PriceManagement.UpdatePriceList", new DateTime(2025, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Chỉnh sửa thông tin bảng giá", new Guid("e4961eb7-931a-4403-b94a-fb2f7860b80e"), true, null, null, null },
                    { new Guid("2fa87083-9448-4fd6-9ff0-17476b164ae9"), "UserGroupManagement.Add", new DateTime(2025, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Thêm mới nhóm người dùng", new Guid("5f09fd6f-3a0b-4c40-85b6-0d5cbd85c3ce"), true, null, null, null },
                    { new Guid("3d925e57-f62a-4d85-b7d5-7a31720d84d4"), "UserManagement.Export", new DateTime(2025, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Tải xuống file thông tin người dùng", new Guid("c9c55ef5-01e0-4fa6-b68a-3aaea1089548"), true, null, null, null },
                    { new Guid("3e3f1397-0860-4b60-9c56-cf01e145e4d4"), "PermissionManagement.Update", new DateTime(2025, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Chỉnh sửa quyền", new Guid("9fc99947-c7a9-44aa-b9eb-ea42b28590fc"), true, null, null, null },
                    { new Guid("41d2342f-72dc-4c07-a35e-d0f3ea01367d"), "GiftManagement.Update", new DateTime(2025, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Chính sửa xóa thông tin quà tặng", new Guid("3d33a7c7-2cf2-4383-9487-e88c0640d12c"), true, null, null, null },
                    { new Guid("4a32d1c9-21f2-48c0-98e0-2f5c908f6c34"), "UserManagement.Delete", new DateTime(2025, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Xóa thông tin người dùng", new Guid("c9c55ef5-01e0-4fa6-b68a-3aaea1089548"), true, null, null, null },
                    { new Guid("4b769b93-94b3-4a4e-83ff-09c29d580d64"), "RoleGroupManagement.Delete", new DateTime(2025, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Xóa nhóm quyền", new Guid("31bafbd0-7346-4aaf-a1fa-bf2194b1be68"), true, null, null, null },
                    { new Guid("4f1f9949-060a-4704-8f6e-4072052fd83e"), "BannerManagement.View", new DateTime(2025, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Xem và tìm kiếm danh sách, thông tin banner", new Guid("d9679732-17a3-44ff-b4ed-b39f631c3e13"), true, null, null, null },
                    { new Guid("6124621e-ecf9-4c80-81cc-48d0d16e22d0"), "StudentProgressManagement.LoginAsUser", new DateTime(2025, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Log in as user", new Guid("39d8915b-4373-4ed2-9a79-d59db3a5285f"), true, null, null, null },
                    { new Guid("6c1fcd70-9e03-4202-9b4a-06cbb5f0f57b"), "VoucherManagement.Delete", new DateTime(2025, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Xóa thông tin voucher", new Guid("57cbd9f0-87d5-4c5c-8986-08e6fae58d99"), true, null, null, null },
                    { new Guid("75d60a3b-7f0d-43c1-b3e6-352b2b313d91"), "PermissionManagement.View", new DateTime(2025, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Xem thông tin quyền", new Guid("9fc99947-c7a9-44aa-b9eb-ea42b28590fc"), true, null, null, null },
                    { new Guid("7d14f6e2-6f80-4e47-bfcd-b60f9ed6d101"), "UserManagement.View", new DateTime(2025, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Xem và tìm kiếm danh sách, thông tin người dùng", new Guid("c9c55ef5-01e0-4fa6-b68a-3aaea1089548"), true, null, null, null },
                    { new Guid("8020537c-8f9f-44c5-89ce-55e1e52a1d9e"), "PermissionManagement.Add", new DateTime(2025, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Thêm mới quyền", new Guid("9fc99947-c7a9-44aa-b9eb-ea42b28590fc"), true, null, null, null },
                    { new Guid("83dc20d0-f42a-4c5b-bc97-2d31b553bbdf"), "RoleGroupManagement.Add", new DateTime(2025, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Thêm mới nhóm quyền", new Guid("31bafbd0-7346-4aaf-a1fa-bf2194b1be68"), true, null, null, null },
                    { new Guid("89b2e2df-df40-4cb9-9f86-7a7fa315a112"), "AuthorizationManagement.View", new DateTime(2025, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Xem bảng phân quyền", new Guid("f9142961-5fe0-4015-8d01-9e12d202a7ae"), true, null, null, null },
                    { new Guid("89b2e2df-df50-6cb6-9f86-7a7fa315a186"), "HomeDashboard.ViewDashboard", new DateTime(2025, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Xem thông tin dashboard", new Guid("89b8e2df-df40-4cb6-9f86-7a7fa315a194"), true, null, null, null },
                    { new Guid("89b8e2df-df40-46b6-9f86-7a73a315a186"), "PopupOrderManagement.Update", new DateTime(2025, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Chỉnh sửa thông tin popup", new Guid("22960350-3a0f-468b-849f-bf0fd7db264b"), true, null, null, null },
                    { new Guid("89b8e2df-df40-4cb2-9f86-7a7fa315a123"), "CourseGoalManagement.Update", new DateTime(2025, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Chỉnh sửa/xóa thông tin cấu hình mục tiêu khóa học", new Guid("df96b682-99c5-4ae2-9516-329d2f2d3054"), true, null, null, null },
                    { new Guid("89b8e2df-df40-4cb6-9f86-7a75a315a186"), "PopupOrderManagement.View", new DateTime(2025, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Xem danh sách popup hiện có", new Guid("22960350-3a0f-468b-849f-bf0fd7db264b"), true, null, null, null },
                    { new Guid("89b8e2df-df40-4cb6-9f86-7a7fa315a122"), "CourseGoalManagement.Add", new DateTime(2025, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Thêm mới các cột mốc trong thông tin cấu hình mục tiêu khóa học", new Guid("df96b682-99c5-4ae2-9516-329d2f2d3054"), true, null, null, null },
                    { new Guid("89b8e2df-df40-4cb7-9f86-7a7fa315a121"), "CourseGoalManagement.View", new DateTime(2025, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Xem thông tin cấu hình mục tiêu khóa học", new Guid("df96b682-99c5-4ae2-9516-329d2f2d3054"), true, null, null, null },
                    { new Guid("89bee2df-df40-44f6-9f86-7a7fa315a186"), "ReferralCodeManagement.View", new DateTime(2025, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Xem và tìm kiếm thông tin mã giới thiệu", new Guid("b652d6d2-f5f0-4c5d-a8b1-ec58860cc6d7"), true, null, null, null },
                    { new Guid("8b2f8f46-68f6-49d2-b0c0-e3c582d56301"), "ForbiddenWordsStorage.Add", new DateTime(2025, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Thêm mới từ cấm", new Guid("e758b51b-74de-4a2b-87b3-394f701bce9f"), true, null, null, null },
                    { new Guid("921cb40a-c0c3-4aa2-b29e-9614cf1bb7b1"), "UserGroupManagement.Delete", new DateTime(2025, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Xóa nhóm người dùng", new Guid("5f09fd6f-3a0b-4c40-85b6-0d5cbd85c3ce"), true, null, null, null },
                    { new Guid("9b0ebf6a-f5d9-4fd2-a87d-73aa4a6c2e11"), "RoleGroupManagement.View", new DateTime(2025, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Xem thông tin nhóm quyền", new Guid("31bafbd0-7346-4aaf-a1fa-bf2194b1be68"), true, null, null, null },
                    { new Guid("9cb2f671-29ae-4218-80b0-4c5a3b22d1b3"), "PriceManagement.UpdatePackage", new DateTime(2025, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Chỉnh sửa thông tin gói bán hàng", new Guid("e4961eb7-931a-4403-b94a-fb2f7860b80e"), true, null, null, null },
                    { new Guid("9e12d4d1-5949-4f62-985f-d50ed2c94b94"), "BannerManagement.EditFrequencyOfAppearance", new DateTime(2025, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Chỉnh sửa tần suất xuất hiện giữa các banner", new Guid("d9679732-17a3-44ff-b4ed-b39f631c3e13"), true, null, null, null },
                    { new Guid("9fc8bbf6-0a62-4a80-9c06-9a1094e3f2de"), "StudentManagement.View", new DateTime(2025, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Xem và tìm kiếm danh sách thông tin học sinh", new Guid("68e5e3d2-90a1-4c60-9b8f-77a03381dc92"), true, null, null, null },
                    { new Guid("a49d92f6-b4df-4c0f-b7ee-bf16b74fef5b"), "GiftManagement.View", new DateTime(2025, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Xem được danh sách và lịch sử các quà tặng", new Guid("3d33a7c7-2cf2-4383-9487-e88c0640d12c"), true, null, null, null },
                    { new Guid("b6e04f5e-090f-4e7f-b127-2ad8dd3df62c"), "PriceManagement.ViewPriceList", new DateTime(2025, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Xem và tìm kiếm thông tin bảng giá", new Guid("e4961eb7-931a-4403-b94a-fb2f7860b80e"), true, null, null, null },
                    { new Guid("b7bacc00-e12e-438e-8e06-30eaa876cf2a"), "PriceManagement.AddPriceList", new DateTime(2025, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Thêm mới bảng giá", new Guid("e4961eb7-931a-4403-b94a-fb2f7860b80e"), true, null, null, null },
                    { new Guid("b9d4227f-9146-4174-b285-dbe307d239d4"), "VoucherManagement.Add", new DateTime(2025, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Thêm voucher mới", new Guid("57cbd9f0-87d5-4c5c-8986-08e6fae58d99"), true, null, null, null },
                    { new Guid("bbdebc34-4918-42b4-bd1e-ea3169188fc7"), "BannerManagement.Preview", new DateTime(2025, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Xem preview banner", new Guid("d9679732-17a3-44ff-b4ed-b39f631c3e13"), true, null, null, null },
                    { new Guid("bcb9f3f3-5383-44f5-8038-2e2b54516dbe"), "BannerManagement.Add", new DateTime(2025, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Tạo mới banner", new Guid("d9679732-17a3-44ff-b4ed-b39f631c3e13"), true, null, null, null },
                    { new Guid("c1cf6e9b-3378-4a84-b040-8bc2a688a3c3"), "PriceManagement.ViewPackage", new DateTime(2025, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Xem danh sách gói bán hàng", new Guid("e4961eb7-931a-4403-b94a-fb2f7860b80e"), true, null, null, null },
                    { new Guid("c34f9166-6c6d-4bb8-9d49-bfb6b9cc9e03"), "GiftManagement.Delete", new DateTime(2025, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Xóa thông tin quà tặng", new Guid("3d33a7c7-2cf2-4383-9487-e88c0640d12c"), true, null, null, null },
                    { new Guid("c79f6403-5033-4691-bd3f-c73a47be79dc"), "UserManagement.Add", new DateTime(2025, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Thêm mới thông tin người dùng", new Guid("c9c55ef5-01e0-4fa6-b68a-3aaea1089548"), true, null, null, null },
                    { new Guid("c9cd8d5b-e86e-49a6-b408-5c40f7305f7e"), "PromotionManagement.Add", new DateTime(2025, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Tạo ưu đãi mới", new Guid("b2b1272c-3b9d-4c6e-91f4-60c302c7d7e1"), true, null, null, null },
                    { new Guid("cb38a1e4-03d5-4a7c-b445-36c8892d8e9a"), "ClassManagement.View", new DateTime(2025, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Xem và tìm kiếm danh sách, thông tin lớp học", new Guid("b0a0b3b1-35aa-4ef5-8c17-95a5b7cf7a2b"), true, null, null, null },
                    { new Guid("cc0e79f6-d0c4-4097-bef3-d6eb1e6cd39c"), "RoleGroupManagement.Update", new DateTime(2025, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Chỉnh sửa nhóm quyền", new Guid("31bafbd0-7346-4aaf-a1fa-bf2194b1be68"), true, null, null, null },
                    { new Guid("cfb04a92-c72e-4f7b-bc84-0b0ebac6b597"), "TaskManagement.Add", new DateTime(2025, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Tạo nhiệm vụ mới", new Guid("95a2cf57-3b18-41b4-b5d5-9291a7bb87a5"), true, null, null, null },
                    { new Guid("d2531479-9bc9-42e7-a716-243ce2b053c2"), "TaskManagement.View", new DateTime(2025, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Xem và tìm kiếm danh sách các nhiệm vụ", new Guid("95a2cf57-3b18-41b4-b5d5-9291a7bb87a5"), true, null, null, null },
                    { new Guid("d553e5b1-0bc3-476d-a593-6bb31b90c184"), "VoucherManagement.Export", new DateTime(2025, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Xuất file voucher", new Guid("57cbd9f0-87d5-4c5c-8986-08e6fae58d99"), true, null, null, null },
                    { new Guid("d826f149-1a31-4ec7-91d3-49e537fc08e5"), "ErrorReportManagement.Update", new DateTime(2025, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Chỉnh sửa thông tin báo lỗi", new Guid("ab4582fd-e613-4904-bdc3-c50ff03a1d4d"), true, null, null, null },
                    { new Guid("de71e5bc-b8cb-4b84-a470-c182bd9b73b3"), "ForbiddenWordsStorage.View", new DateTime(2025, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Xen thông tin kho từ cấm", new Guid("e758b51b-74de-4a2b-87b3-394f701bce9f"), true, null, null, null },
                    { new Guid("e4c3e95b-4a1d-45a6-87c5-537801c4f7e2"), "GiftManagement.Export", new DateTime(2025, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Xuất danh sách thông tin đổi quà", new Guid("3d33a7c7-2cf2-4383-9487-e88c0640d12c"), true, null, null, null },
                    { new Guid("e4cf2c12-44c5-46f6-b0a4-370bdb6a5d43"), "PromotionManagement.View", new DateTime(2025, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Xem và tìm kiếm danh sách các ưu đãi", new Guid("b2b1272c-3b9d-4c6e-91f4-60c302c7d7e1"), true, null, null, null },
                    { new Guid("e4e6aeac-f23d-41d5-b5e4-18ad27d20bb9"), "VoucherManagement.View", new DateTime(2025, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Tìm kiếm và xem thông tin voucher", new Guid("57cbd9f0-87d5-4c5c-8986-08e6fae58d99"), true, null, null, null },
                    { new Guid("e8d09769-181f-4483-bf4b-4f03d13c4537"), "StudentProgressManagement.View", new DateTime(2025, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Xem và tìm kiếm danh sách tiến độ của học sinh", new Guid("39d8915b-4373-4ed2-9a79-d59db3a5285f"), true, null, null, null },
                    { new Guid("ec2cf5cc-f202-4de1-bdee-89dcac21d0d9"), "PaymentManagement.View", new DateTime(2025, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Xem và tìm kiếm danh sách, thông tin thanh toán", new Guid("b369b45f-2b8e-4cb0-91c2-2ad73d29d88d"), true, null, null, null },
                    { new Guid("f03891a1-6b57-4f58-a25b-d9b249654315"), "VoucherManagement.Update", new DateTime(2025, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Chỉnh sửa thông tin voucher", new Guid("57cbd9f0-87d5-4c5c-8986-08e6fae58d99"), true, null, null, null },
                    { new Guid("f1db5cd3-0a63-43e5-9ac5-3cf52495d4cd"), "ClassManagement.Update", new DateTime(2025, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Chỉnh sửa thông tin lớp học", new Guid("b0a0b3b1-35aa-4ef5-8c17-95a5b7cf7a2b"), true, null, null, null },
                    { new Guid("f3087b7a-4215-4041-9f70-3a9b4bb6cbe9"), "StudentManagement.Export", new DateTime(2025, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Xuất dữ liệu danh sách học sinh", new Guid("68e5e3d2-90a1-4c60-9b8f-77a03381dc92"), true, null, null, null },
                    { new Guid("f51e98e1-30a5-4da2-9b6b-cb395116ed56"), "UserGroupManagement.View", new DateTime(2025, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Xem thông tin nhóm người dùng", new Guid("5f09fd6f-3a0b-4c40-85b6-0d5cbd85c3ce"), true, null, null, null },
                    { new Guid("fa343a8b-6ef9-48bb-94fa-2c20a6bc4e12"), "UserManagement.Update", new DateTime(2025, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Chỉnh sửa thông tin người dùng", new Guid("c9c55ef5-01e0-4fa6-b68a-3aaea1089548"), true, null, null, null },
                    { new Guid("fb00742a-82db-42f7-b63f-4a5dbf0b9de0"), "PriceManagement.AddPackage", new DateTime(2025, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Thêm mới gói bán hàng", new Guid("e4961eb7-931a-4403-b94a-fb2f7860b80e"), true, null, null, null },
                    { new Guid("fbe61a77-69e7-40a9-bbff-c76f5b49f2f9"), "PaymentManagement.Update", new DateTime(2025, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, false, "Phê duyệt/Từ chối thanh toán", new Guid("b369b45f-2b8e-4cb0-91c2-2ad73d29d88d"), true, null, null, null }
                });

            migrationBuilder.InsertData(
                table: "AspNetRoleClaims",
                columns: new[] { "Id", "ClaimType", "ClaimValue", "PermissionGroupId", "PermissionId", "RoleId", "Status" },
                values: new object[,]
                {
                    { 1, "AuthorizationManagement", "AuthorizationManagement.View", new Guid("f9142961-5fe0-4015-8d01-9e12d202a7ae"), new Guid("89b2e2df-df40-4cb9-9f86-7a7fa315a112"), new Guid("69976022-5dbb-4292-bab6-e94b6701061e"), true },
                    { 2, "AuthorizationManagement", "AuthorizationManagement.Update", new Guid("f9142961-5fe0-4015-8d01-9e12d202a7ae"), new Guid("19b8e2df-d240-4cb6-9f86-7a7fa315a186"), new Guid("69976022-5dbb-4292-bab6-e94b6701061e"), true }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_PermissionGroupId",
                table: "AspNetRoleClaims",
                column: "PermissionGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_PermissionId",
                table: "AspNetRoleClaims",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_PermissionGroupId",
                table: "Permissions",
                column: "PermissionGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetRoleClaims_PermissionGroups_PermissionGroupId",
                table: "AspNetRoleClaims",
                column: "PermissionGroupId",
                principalTable: "PermissionGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetRoleClaims_Permissions_PermissionId",
                table: "AspNetRoleClaims",
                column: "PermissionId",
                principalTable: "Permissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetRoleClaims_PermissionGroups_PermissionGroupId",
                table: "AspNetRoleClaims");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetRoleClaims_Permissions_PermissionId",
                table: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "PermissionGroups");

            migrationBuilder.DropIndex(
                name: "IX_AspNetRoleClaims_PermissionGroupId",
                table: "AspNetRoleClaims");

            migrationBuilder.DropIndex(
                name: "IX_AspNetRoleClaims_PermissionId",
                table: "AspNetRoleClaims");

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DropColumn(
                name: "PermissionGroupId",
                table: "AspNetRoleClaims");

            migrationBuilder.DropColumn(
                name: "PermissionId",
                table: "AspNetRoleClaims");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "AspNetRoleClaims");
        }
    }
}
