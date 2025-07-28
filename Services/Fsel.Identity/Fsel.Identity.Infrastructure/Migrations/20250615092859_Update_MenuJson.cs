using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Identity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_MenuJson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: new Guid("049a2515-641d-447b-b27b-e46f41e64ef3"),
                column: "ConfigStr",
                value: "{\"id\":16,\"code_title\":\"HOME_PAGE.STUDENT_PROGRESS_MANAGEMENT\",\"link\":\"/student-progress\",\"icon\":\"payment-icon.png\",\"parentId\":0,\"code\":\"\",\"permission\":\"Admin,CSO\",\"children\":[]}");

            migrationBuilder.UpdateData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: new Guid("06641b23-3857-4b6d-b1d7-096fc98fa8b9"),
                column: "ConfigStr",
                value: "{\"id\":4,\"code_title\":\"HOME_PAGE.STUDENT_MANAGEMENT\",\"link\":\"/student\",\"icon\":\"Students.svg\",\"parentId\":0,\"code\":\"\",\"permission\":\"AdminSchool,CSO\",\"children\":[]}");

            migrationBuilder.UpdateData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: new Guid("06e2c9b5-378b-4a32-a36f-45a65b68d791"),
                column: "ConfigStr",
                value: "{\"id\":8,\"code_title\":\"HOME_PAGE.EXERCISE_MANAGEMENT\",\"link\":\"/exercise-management\",\"icon\":\"Edit.svg\",\"parentId\":0,\"code\":\"\",\"permission\":\"Teacher\",\"children\":[]}");

            migrationBuilder.UpdateData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: new Guid("121436b1-ac91-46c2-b4b8-76da74e13d33"),
                column: "ConfigStr",
                value: "{\"id\":9,\"code_title\":\"HOME_PAGE.MANAGE_REVIEWS\",\"link\":\"/review\",\"icon\":\"Review.svg\",\"parentId\":0,\"code\":\"\",\"permission\":\"Admin\",\"children\":[]}");

            migrationBuilder.UpdateData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: new Guid("17234df3-b92f-4103-9bc0-7b5feaad7b00"),
                column: "ConfigStr",
                value: "{\"id\":7,\"code_title\":\"HOME_PAGE.EMPTY_CALENDAR\",\"link\":\"/empty-calendar\",\"icon\":\"Livestream.svg\",\"parentId\":0,\"code\":\"\",\"permission\":\"CSO,Teacher\",\"children\":[]}");

            migrationBuilder.UpdateData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: new Guid("18e09c83-8f23-4509-a39c-7d3d32c4d81b"),
                column: "ConfigStr",
                value: "{\"id\":24,\"code_title\":\"Quản lý báo cáo\",\"link\":\"/report-management\",\"icon\":\"Star.png\",\"parentId\":0,\"code\":\"\",\"permission\":\"AdminSchool\",\"children\":[{\"id\":220,\"code_title\":\"Kết quả đánh giá đầu vào\",\"link\":\"/report-management/pt-result\",\"icon\":\"\",\"parentId\":22,\"code\":\"\",\"permission\":\"AdminSchool\",\"children\":null},{\"id\":221,\"code_title\":\"Tiến độ học tập\",\"link\":\"/report-management/learning-progress\",\"icon\":\"\",\"parentId\":22,\"code\":\"\",\"permission\":\"AdminSchool\",\"children\":null},{\"id\":222,\"code_title\":\"Kết quả học tập\",\"link\":\"/report-management/learning-result\",\"icon\":\"\",\"parentId\":22,\"code\":\"\",\"permission\":\"AdminSchool\",\"children\":null},{\"id\":223,\"code_title\":\"Chuyên cần\",\"link\":\"/report-management/assiduity\",\"icon\":\"\",\"parentId\":22,\"code\":\"\",\"permission\":\"AdminSchool\",\"children\":null}]}");

            migrationBuilder.UpdateData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: new Guid("19f86ebc-e6e9-4baf-84d8-f0e90489b2a1"),
                column: "ConfigStr",
                value: "{\"id\":29,\"code_title\":\"Quản lý Banner\",\"link\":\"/banner\",\"icon\":\"Star.png\",\"parentId\":0,\"code\":\"\",\"permission\":\"Admin\",\"children\":[]}");

            migrationBuilder.UpdateData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: new Guid("1c9b88c3-c865-4d91-89df-3b84fec63843"),
                column: "ConfigStr",
                value: "{\"id\":22,\"code_title\":\"Quản lý voucher\",\"link\":\"/voucher\",\"icon\":\"Star.png\",\"parentId\":0,\"code\":\"\",\"permission\":\"Admin\",\"children\":[]}");

            migrationBuilder.UpdateData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: new Guid("2d1222ff-8e49-4689-8c47-c6ebca08fb61"),
                column: "ConfigStr",
                value: "{\"id\":11,\"code_title\":\"HOME_PAGE.STORE_OF_FORBIDDEN_WORDS\",\"link\":\"/forbidden-word\",\"icon\":\"Forbiden.svg\",\"parentId\":0,\"code\":\"\",\"permission\":\"Admin\",\"children\":[]}");

            migrationBuilder.UpdateData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: new Guid("3e1dfc7e-a768-4316-ae01-00a20cd4a281"),
                column: "ConfigStr",
                value: "{\"id\":2,\"code_title\":\"HOME_PAGE.PERSONAL_INFORMATION\",\"link\":\"/user/detail\",\"icon\":\"People.svg\",\"parentId\":0,\"code\":\"\",\"permission\":\"Teacher,CSO\",\"children\":[]}");

            migrationBuilder.UpdateData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: new Guid("464add9c-629a-4a83-99e0-70d020d72a04"),
                column: "ConfigStr",
                value: "{\"id\":21,\"code_title\":\"Quản lý mã giới thiệu\",\"link\":\"/invite-friends\",\"icon\":\"Star.png\",\"parentId\":0,\"code\":\"\",\"permission\":\"Admin\",\"children\":[]}");

            migrationBuilder.UpdateData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: new Guid("4aff9531-9c38-4ef9-a965-923585d9d47a"),
                column: "ConfigStr",
                value: "{\"id\":15,\"code_title\":\"HOME_PAGE.QUEST_BOARD_MANAGEMENT\",\"link\":\"/quest-board\",\"icon\":\"payment-icon.png\",\"parentId\":0,\"code\":\"\",\"permission\":\"Admin\",\"children\":[]}");

            migrationBuilder.UpdateData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: new Guid("53416e56-89d7-4fec-bb3f-c9225878470d"),
                column: "ConfigStr",
                value: "{\"id\":26,\"code_title\":\"Thêm học sinh vào túi mù\",\"link\":\"/blind-box\",\"icon\":\"Star.png\",\"parentId\":0,\"code\":\"\",\"permission\":\"Admin\",\"children\":[]}");

            migrationBuilder.UpdateData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: new Guid("56f3a987-a5cf-4eee-88b7-baac8a82caee"),
                column: "ConfigStr",
                value: "{\"id\":4,\"code_title\":\"HOME_PAGE.STUDENT_MANAGEMENT\",\"link\":\"/student-admin\",\"icon\":\"Students.svg\",\"parentId\":0,\"code\":\"\",\"permission\":\"Admin\",\"children\":[]}");

            migrationBuilder.UpdateData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: new Guid("600f483b-dd9e-4aa3-9364-bbdca16a391a"),
                column: "ConfigStr",
                value: "{\"id\":23,\"code_title\":\"Cấu hình thứ tự popup\",\"link\":\"/popup-order-config\",\"icon\":\"Star.png\",\"parentId\":0,\"code\":\"\",\"permission\":\"Admin\",\"children\":[]}");

            migrationBuilder.UpdateData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: new Guid("655bbf11-95ad-4941-8739-878437acb3c2"),
                column: "ConfigStr",
                value: "{\"id\":18,\"code_title\":\"HOME_PAGE.HELP_AND_SUPPORT_MANAGEMENT\",\"link\":\"/help-and-support-management\",\"icon\":\"Star.png\",\"parentId\":0,\"code\":\"\",\"permission\":\"Admin\",\"children\":[]}");

            migrationBuilder.UpdateData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: new Guid("66b606c5-ef09-4f76-89e4-e11d0d914cb5"),
                column: "ConfigStr",
                value: "{\"id\":27,\"code_title\":\"Hướng dẫn sử dụng\",\"link\":\"/guide\",\"icon\":\"Star.png\",\"parentId\":0,\"code\":\"\",\"permission\":\"Admin,CSO,AdminSchool\",\"children\":[]}");

            migrationBuilder.UpdateData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: new Guid("68365a87-73fa-4926-acb6-24f1403f77ad"),
                column: "ConfigStr",
                value: "{\"id\":25,\"code_title\":\"Dashboard\",\"link\":\"/dashboard\",\"icon\":\"Star.png\",\"parentId\":0,\"code\":\"\",\"permission\":\"AdminSchool\",\"children\":[{\"id\":250,\"code_title\":\"Báo cáo tiến độ học tập\",\"link\":\"/dashboard/progress-learn\",\"icon\":\"\",\"parentId\":25,\"code\":\"\",\"permission\":\"AdminSchool\",\"children\":null},{\"id\":251,\"code_title\":\"Báo cáo kết quả đánh giá đầu vào\",\"link\":\"/dashboard/result-pt\",\"icon\":\"\",\"parentId\":25,\"code\":\"\",\"permission\":\"AdminSchool\",\"children\":null},{\"id\":252,\"code_title\":\"Báo cáo kết quả học tập\",\"link\":\"/dashboard/result-learn\",\"icon\":\"\",\"parentId\":25,\"code\":\"\",\"permission\":\"AdminSchool\",\"children\":null},{\"id\":253,\"code_title\":\"Chuyên cần\",\"link\":\"/dashboard/diligence\",\"icon\":\"\",\"parentId\":25,\"code\":\"\",\"permission\":\"AdminSchool\",\"children\":null}]}");

            migrationBuilder.UpdateData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: new Guid("6bd509c9-3986-4780-8ea0-e02059c9d9e9"),
                column: "ConfigStr",
                value: "{\"id\":19,\"code_title\":\"HOME_PAGE.ERROR_REPORT_MANAGEMENT\",\"link\":\"/error-reporting-management\",\"icon\":\"Star.png\",\"parentId\":0,\"code\":\"\",\"permission\":\"Admin\",\"children\":[]}");

            migrationBuilder.UpdateData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: new Guid("6ee1bea6-02ea-4502-b8cc-92671e6430fc"),
                column: "ConfigStr",
                value: "{\"id\":20,\"code_title\":\"HOME_PAGE.PRICE_LIST_MANAGEMENT\",\"link\":\"/price-management\",\"icon\":\"Star.png\",\"parentId\":0,\"code\":\"\",\"permission\":\"Admin\",\"children\":[]}");

            migrationBuilder.UpdateData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: new Guid("77eee0bb-70b0-4672-964c-35adb0a4df6f"),
                column: "ConfigStr",
                value: "{\"id\":1,\"code_title\":\"HOME_PAGE.HOME\",\"link\":\"/home\",\"icon\":\"Home.svg\",\"parentId\":0,\"code\":\"\",\"permission\":\"\",\"children\":[]}");

            migrationBuilder.UpdateData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: new Guid("b50e1cd9-5b46-467a-ba7f-b86011747b32"),
                column: "ConfigStr",
                value: "{\"id\":6,\"code_title\":\"HOME_PAGE.LIVE_CLASS\",\"link\":\"/live-class\",\"icon\":\"Livestream.svg\",\"parentId\":0,\"code\":\"\",\"permission\":\"CSO,Teacher\",\"children\":[]}");

            migrationBuilder.UpdateData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: new Guid("be37f400-f10b-40b6-bc01-ad0e3d114fce"),
                column: "ConfigStr",
                value: "{\"id\":229,\"code_title\":\"Quản lý mục tiêu khóa học\",\"link\":\"/course-target\",\"icon\":\"Star.png\",\"parentId\":0,\"code\":\"\",\"permission\":\"Admin\",\"children\":[]}");

            migrationBuilder.UpdateData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: new Guid("c13b856d-a3ee-40fa-a018-2d33ce028f6a"),
                column: "ConfigStr",
                value: "{\"id\":12,\"code_title\":\"HOME_PAGE.PAYMENT_TITLE\",\"link\":\"/payment\",\"icon\":\"payment-icon.png\",\"parentId\":0,\"code\":\"\",\"permission\":\"Admin\",\"children\":[]}");

            migrationBuilder.UpdateData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: new Guid("c3851e05-cda0-4418-b852-8304949104a0"),
                column: "ConfigStr",
                value: "{\"id\":14,\"code_title\":\"HOME_PAGE.CLASS_FORUM_MANAGEMENT\",\"link\":\"/class-forum-management\",\"icon\":\"Chat.svg\",\"parentId\":0,\"code\":\"\",\"permission\":\"CSO\",\"children\":[]}");

            migrationBuilder.UpdateData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: new Guid("cd067ac8-af65-47bd-bbd4-0bf3b9ad90fb"),
                column: "ConfigStr",
                value: "{\"id\":13,\"code_title\":\"HOME_PAGE.VOUCHER-MANAGEMENT\",\"link\":\"/voucher-management\",\"icon\":\"payment-icon.png\",\"parentId\":0,\"code\":\"\",\"permission\":\"Admin\",\"children\":[]}");

            migrationBuilder.UpdateData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: new Guid("d5b153cc-1481-40d3-b39f-b35a67dd54e6"),
                column: "ConfigStr",
                value: "{\"id\":3,\"code_title\":\"HOME_PAGE.CLASS_MANAGER\",\"link\":\"/class\",\"icon\":\"Presentation.svg\",\"parentId\":0,\"code\":\"\",\"permission\":\"Admin,CSO\",\"children\":[]}");

            migrationBuilder.UpdateData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: new Guid("de64d9a6-6011-4a25-b73f-af31a0c530a1"),
                column: "ConfigStr",
                value: "{\"id\":2,\"code_title\":\"HOME_PAGE.USER_MANAGEMENT\",\"link\":\"/user\",\"icon\":\"People.svg\",\"parentId\":0,\"code\":\"\",\"permission\":\"Admin\",\"children\":[]}");

            migrationBuilder.UpdateData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: new Guid("e7b3e33f-c1cb-48e7-bbe0-39be74dd5e36"),
                column: "ConfigStr",
                value: "{\"id\":30,\"code_title\":\"Quản lý quà tặng FSEL\",\"link\":\"/gift\",\"icon\":\"Star.png\",\"parentId\":0,\"code\":\"\",\"permission\":\"Admin\",\"children\":[]}");

            migrationBuilder.UpdateData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: new Guid("efb2d814-6b8b-4c12-92a4-21fde9feeb54"),
                column: "ConfigStr",
                value: "{\"id\":27,\"code_title\":\"i18n_user_permissions\",\"link\":\"/user-permissions\",\"icon\":\"Star.png\",\"parentId\":0,\"code\":\"\",\"permission\":\"Admin\",\"children\":[{\"id\":270,\"code_title\":\"i18n_user_group\",\"link\":\"/user-permissions/user-group\",\"icon\":\"\",\"parentId\":27,\"code\":\"\",\"permission\":\"Admin\",\"children\":null},{\"id\":271,\"code_title\":\"i18n_permission_group\",\"link\":\"/user-permissions/permission-group\",\"icon\":\"\",\"parentId\":27,\"code\":\"\",\"permission\":\"Admin\",\"children\":null},{\"id\":272,\"code_title\":\"i18n_permission\",\"link\":\"/user-permissions/permission-list\",\"icon\":\"\",\"parentId\":27,\"code\":\"\",\"permission\":\"Admin\",\"children\":null},{\"id\":273,\"code_title\":\"i18n_permission_access\",\"link\":\"/user-permissions/permission-access\",\"icon\":\"\",\"parentId\":27,\"code\":\"\",\"permission\":\"Admin\",\"children\":null}]}");

            migrationBuilder.UpdateData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: new Guid("f768e305-26aa-4bca-9197-c3673825373e"),
                column: "ConfigStr",
                value: "{\"id\":5,\"code_title\":\"HOME_PAGE.MANAGE_LIVE_TIME_FRAMES\",\"link\":\"/live-time\",\"icon\":\"Livestream.svg\",\"parentId\":0,\"code\":\"\",\"permission\":\"Admin\",\"children\":[]}");

            migrationBuilder.UpdateData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: new Guid("f8759866-2981-4751-a447-d6c0230fa44a"),
                column: "ConfigStr",
                value: "{\"id\":10,\"code_title\":\"HOME_PAGE.SET_TIME\",\"link\":\"/setup-time\",\"icon\":\"Clock.svg\",\"parentId\":0,\"code\":\"\",\"permission\":\"Admin\",\"children\":[]}");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: new Guid("049a2515-641d-447b-b27b-e46f41e64ef3"),
                column: "ConfigStr",
                value: "{\"id\":16,\"code_title\":null,\"link\":\"/student-progress\",\"icon\":\"payment-icon.png\",\"parentId\":0,\"code\":\"\",\"permission\":\"Admin,CSO\",\"children\":[]}");

            migrationBuilder.UpdateData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: new Guid("06641b23-3857-4b6d-b1d7-096fc98fa8b9"),
                column: "ConfigStr",
                value: "{\"id\":4,\"code_title\":null,\"link\":\"/student\",\"icon\":\"Students.svg\",\"parentId\":0,\"code\":\"\",\"permission\":\"AdminSchool,CSO\",\"children\":[]}");

            migrationBuilder.UpdateData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: new Guid("06e2c9b5-378b-4a32-a36f-45a65b68d791"),
                column: "ConfigStr",
                value: "{\"id\":8,\"code_title\":null,\"link\":\"/exercise-management\",\"icon\":\"Edit.svg\",\"parentId\":0,\"code\":\"\",\"permission\":\"Teacher\",\"children\":[]}");

            migrationBuilder.UpdateData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: new Guid("121436b1-ac91-46c2-b4b8-76da74e13d33"),
                column: "ConfigStr",
                value: "{\"id\":9,\"code_title\":null,\"link\":\"/review\",\"icon\":\"Review.svg\",\"parentId\":0,\"code\":\"\",\"permission\":\"Admin\",\"children\":[]}");

            migrationBuilder.UpdateData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: new Guid("17234df3-b92f-4103-9bc0-7b5feaad7b00"),
                column: "ConfigStr",
                value: "{\"id\":7,\"code_title\":null,\"link\":\"/empty-calendar\",\"icon\":\"Livestream.svg\",\"parentId\":0,\"code\":\"\",\"permission\":\"CSO,Teacher\",\"children\":[]}");

            migrationBuilder.UpdateData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: new Guid("18e09c83-8f23-4509-a39c-7d3d32c4d81b"),
                column: "ConfigStr",
                value: "{\"id\":24,\"code_title\":null,\"link\":\"/report-management\",\"icon\":\"Star.png\",\"parentId\":0,\"code\":\"\",\"permission\":\"AdminSchool\",\"children\":[{\"id\":220,\"code_title\":null,\"link\":\"/report-management/pt-result\",\"icon\":\"\",\"parentId\":22,\"code\":\"\",\"permission\":\"AdminSchool\",\"children\":null},{\"id\":221,\"code_title\":null,\"link\":\"/report-management/learning-progress\",\"icon\":\"\",\"parentId\":22,\"code\":\"\",\"permission\":\"AdminSchool\",\"children\":null},{\"id\":222,\"code_title\":null,\"link\":\"/report-management/learning-result\",\"icon\":\"\",\"parentId\":22,\"code\":\"\",\"permission\":\"AdminSchool\",\"children\":null},{\"id\":223,\"code_title\":null,\"link\":\"/report-management/assiduity\",\"icon\":\"\",\"parentId\":22,\"code\":\"\",\"permission\":\"AdminSchool\",\"children\":null}]}");

            migrationBuilder.UpdateData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: new Guid("19f86ebc-e6e9-4baf-84d8-f0e90489b2a1"),
                column: "ConfigStr",
                value: "{\"id\":29,\"code_title\":null,\"link\":\"/banner\",\"icon\":\"Star.png\",\"parentId\":0,\"code\":\"\",\"permission\":\"Admin\",\"children\":[]}");

            migrationBuilder.UpdateData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: new Guid("1c9b88c3-c865-4d91-89df-3b84fec63843"),
                column: "ConfigStr",
                value: "{\"id\":22,\"code_title\":null,\"link\":\"/voucher\",\"icon\":\"Star.png\",\"parentId\":0,\"code\":\"\",\"permission\":\"Admin\",\"children\":[]}");

            migrationBuilder.UpdateData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: new Guid("2d1222ff-8e49-4689-8c47-c6ebca08fb61"),
                column: "ConfigStr",
                value: "{\"id\":11,\"code_title\":null,\"link\":\"/forbidden-word\",\"icon\":\"Forbiden.svg\",\"parentId\":0,\"code\":\"\",\"permission\":\"Admin\",\"children\":[]}");

            migrationBuilder.UpdateData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: new Guid("3e1dfc7e-a768-4316-ae01-00a20cd4a281"),
                column: "ConfigStr",
                value: "{\"id\":2,\"code_title\":null,\"link\":\"/user/detail\",\"icon\":\"People.svg\",\"parentId\":0,\"code\":\"\",\"permission\":\"Teacher,CSO\",\"children\":[]}");

            migrationBuilder.UpdateData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: new Guid("464add9c-629a-4a83-99e0-70d020d72a04"),
                column: "ConfigStr",
                value: "{\"id\":21,\"code_title\":null,\"link\":\"/invite-friends\",\"icon\":\"Star.png\",\"parentId\":0,\"code\":\"\",\"permission\":\"Admin\",\"children\":[]}");

            migrationBuilder.UpdateData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: new Guid("4aff9531-9c38-4ef9-a965-923585d9d47a"),
                column: "ConfigStr",
                value: "{\"id\":15,\"code_title\":null,\"link\":\"/quest-board\",\"icon\":\"payment-icon.png\",\"parentId\":0,\"code\":\"\",\"permission\":\"Admin\",\"children\":[]}");

            migrationBuilder.UpdateData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: new Guid("53416e56-89d7-4fec-bb3f-c9225878470d"),
                column: "ConfigStr",
                value: "{\"id\":26,\"code_title\":null,\"link\":\"/blind-box\",\"icon\":\"Star.png\",\"parentId\":0,\"code\":\"\",\"permission\":\"Admin\",\"children\":[]}");

            migrationBuilder.UpdateData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: new Guid("56f3a987-a5cf-4eee-88b7-baac8a82caee"),
                column: "ConfigStr",
                value: "{\"id\":4,\"code_title\":null,\"link\":\"/student-admin\",\"icon\":\"Students.svg\",\"parentId\":0,\"code\":\"\",\"permission\":\"Admin\",\"children\":[]}");

            migrationBuilder.UpdateData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: new Guid("600f483b-dd9e-4aa3-9364-bbdca16a391a"),
                column: "ConfigStr",
                value: "{\"id\":23,\"code_title\":null,\"link\":\"/popup-order-config\",\"icon\":\"Star.png\",\"parentId\":0,\"code\":\"\",\"permission\":\"Admin\",\"children\":[]}");

            migrationBuilder.UpdateData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: new Guid("655bbf11-95ad-4941-8739-878437acb3c2"),
                column: "ConfigStr",
                value: "{\"id\":18,\"code_title\":null,\"link\":\"/help-and-support-management\",\"icon\":\"Star.png\",\"parentId\":0,\"code\":\"\",\"permission\":\"Admin\",\"children\":[]}");

            migrationBuilder.UpdateData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: new Guid("66b606c5-ef09-4f76-89e4-e11d0d914cb5"),
                column: "ConfigStr",
                value: "{\"id\":27,\"code_title\":null,\"link\":\"/guide\",\"icon\":\"Star.png\",\"parentId\":0,\"code\":\"\",\"permission\":\"Admin,CSO,AdminSchool\",\"children\":[]}");

            migrationBuilder.UpdateData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: new Guid("68365a87-73fa-4926-acb6-24f1403f77ad"),
                column: "ConfigStr",
                value: "{\"id\":25,\"code_title\":null,\"link\":\"/dashboard\",\"icon\":\"Star.png\",\"parentId\":0,\"code\":\"\",\"permission\":\"AdminSchool\",\"children\":[{\"id\":250,\"code_title\":null,\"link\":\"/dashboard/progress-learn\",\"icon\":\"\",\"parentId\":25,\"code\":\"\",\"permission\":\"AdminSchool\",\"children\":null},{\"id\":251,\"code_title\":null,\"link\":\"/dashboard/result-pt\",\"icon\":\"\",\"parentId\":25,\"code\":\"\",\"permission\":\"AdminSchool\",\"children\":null},{\"id\":252,\"code_title\":null,\"link\":\"/dashboard/result-learn\",\"icon\":\"\",\"parentId\":25,\"code\":\"\",\"permission\":\"AdminSchool\",\"children\":null},{\"id\":253,\"code_title\":null,\"link\":\"/dashboard/diligence\",\"icon\":\"\",\"parentId\":25,\"code\":\"\",\"permission\":\"AdminSchool\",\"children\":null}]}");

            migrationBuilder.UpdateData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: new Guid("6bd509c9-3986-4780-8ea0-e02059c9d9e9"),
                column: "ConfigStr",
                value: "{\"id\":19,\"code_title\":null,\"link\":\"/error-reporting-management\",\"icon\":\"Star.png\",\"parentId\":0,\"code\":\"\",\"permission\":\"Admin\",\"children\":[]}");

            migrationBuilder.UpdateData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: new Guid("6ee1bea6-02ea-4502-b8cc-92671e6430fc"),
                column: "ConfigStr",
                value: "{\"id\":20,\"code_title\":null,\"link\":\"/price-management\",\"icon\":\"Star.png\",\"parentId\":0,\"code\":\"\",\"permission\":\"Admin\",\"children\":[]}");

            migrationBuilder.UpdateData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: new Guid("77eee0bb-70b0-4672-964c-35adb0a4df6f"),
                column: "ConfigStr",
                value: "{\"id\":1,\"code_title\":null,\"link\":\"/home\",\"icon\":\"Home.svg\",\"parentId\":0,\"code\":\"\",\"permission\":\"\",\"children\":[]}");

            migrationBuilder.UpdateData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: new Guid("b50e1cd9-5b46-467a-ba7f-b86011747b32"),
                column: "ConfigStr",
                value: "{\"id\":6,\"code_title\":null,\"link\":\"/live-class\",\"icon\":\"Livestream.svg\",\"parentId\":0,\"code\":\"\",\"permission\":\"CSO,Teacher\",\"children\":[]}");

            migrationBuilder.UpdateData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: new Guid("be37f400-f10b-40b6-bc01-ad0e3d114fce"),
                column: "ConfigStr",
                value: "{\"id\":229,\"code_title\":null,\"link\":\"/course-target\",\"icon\":\"Star.png\",\"parentId\":0,\"code\":\"\",\"permission\":\"Admin\",\"children\":[]}");

            migrationBuilder.UpdateData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: new Guid("c13b856d-a3ee-40fa-a018-2d33ce028f6a"),
                column: "ConfigStr",
                value: "{\"id\":12,\"code_title\":null,\"link\":\"/payment\",\"icon\":\"payment-icon.png\",\"parentId\":0,\"code\":\"\",\"permission\":\"Admin\",\"children\":[]}");

            migrationBuilder.UpdateData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: new Guid("c3851e05-cda0-4418-b852-8304949104a0"),
                column: "ConfigStr",
                value: "{\"id\":14,\"code_title\":null,\"link\":\"/class-forum-management\",\"icon\":\"Chat.svg\",\"parentId\":0,\"code\":\"\",\"permission\":\"CSO\",\"children\":[]}");

            migrationBuilder.UpdateData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: new Guid("cd067ac8-af65-47bd-bbd4-0bf3b9ad90fb"),
                column: "ConfigStr",
                value: "{\"id\":13,\"code_title\":null,\"link\":\"/voucher-management\",\"icon\":\"payment-icon.png\",\"parentId\":0,\"code\":\"\",\"permission\":\"Admin\",\"children\":[]}");

            migrationBuilder.UpdateData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: new Guid("d5b153cc-1481-40d3-b39f-b35a67dd54e6"),
                column: "ConfigStr",
                value: "{\"id\":3,\"code_title\":null,\"link\":\"/class\",\"icon\":\"Presentation.svg\",\"parentId\":0,\"code\":\"\",\"permission\":\"Admin,CSO\",\"children\":[]}");

            migrationBuilder.UpdateData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: new Guid("de64d9a6-6011-4a25-b73f-af31a0c530a1"),
                column: "ConfigStr",
                value: "{\"id\":2,\"code_title\":null,\"link\":\"/user\",\"icon\":\"People.svg\",\"parentId\":0,\"code\":\"\",\"permission\":\"Admin\",\"children\":[]}");

            migrationBuilder.UpdateData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: new Guid("e7b3e33f-c1cb-48e7-bbe0-39be74dd5e36"),
                column: "ConfigStr",
                value: "{\"id\":30,\"code_title\":null,\"link\":\"/gift\",\"icon\":\"Star.png\",\"parentId\":0,\"code\":\"\",\"permission\":\"Admin\",\"children\":[]}");

            migrationBuilder.UpdateData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: new Guid("efb2d814-6b8b-4c12-92a4-21fde9feeb54"),
                column: "ConfigStr",
                value: "{\"id\":27,\"code_title\":null,\"link\":\"/user-permissions\",\"icon\":\"Star.png\",\"parentId\":0,\"code\":\"\",\"permission\":\"Admin\",\"children\":[{\"id\":270,\"code_title\":null,\"link\":\"/user-permissions/user-group\",\"icon\":\"\",\"parentId\":27,\"code\":\"\",\"permission\":\"Admin\",\"children\":null},{\"id\":271,\"code_title\":null,\"link\":\"/user-permissions/permission-group\",\"icon\":\"\",\"parentId\":27,\"code\":\"\",\"permission\":\"Admin\",\"children\":null},{\"id\":272,\"code_title\":null,\"link\":\"/user-permissions/permission-list\",\"icon\":\"\",\"parentId\":27,\"code\":\"\",\"permission\":\"Admin\",\"children\":null},{\"id\":273,\"code_title\":null,\"link\":\"/user-permissions/permission-access\",\"icon\":\"\",\"parentId\":27,\"code\":\"\",\"permission\":\"Admin\",\"children\":null}]}");

            migrationBuilder.UpdateData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: new Guid("f768e305-26aa-4bca-9197-c3673825373e"),
                column: "ConfigStr",
                value: "{\"id\":5,\"code_title\":null,\"link\":\"/live-time\",\"icon\":\"Livestream.svg\",\"parentId\":0,\"code\":\"\",\"permission\":\"Admin\",\"children\":[]}");

            migrationBuilder.UpdateData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: new Guid("f8759866-2981-4751-a447-d6c0230fa44a"),
                column: "ConfigStr",
                value: "{\"id\":10,\"code_title\":null,\"link\":\"/setup-time\",\"icon\":\"Clock.svg\",\"parentId\":0,\"code\":\"\",\"permission\":\"Admin\",\"children\":[]}");
        }
    }
}
