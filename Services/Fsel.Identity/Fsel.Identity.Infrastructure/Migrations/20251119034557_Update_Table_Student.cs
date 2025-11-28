using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Identity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_Table_Student : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Students_IsDeleted_SchoolId_SchoolClass",
                table: "Students");

            migrationBuilder.AddColumn<string>(
                name: "ClassCampusCode",
                table: "Students",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StudentCampusCode",
                table: "Students",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: new Guid("68365a87-73fa-4926-acb6-24f1403f77ad"),
                column: "ConfigStr",
                value: "{\"id\":25,\"code_title\":\"Dashboard\",\"link\":\"/dashboard\",\"icon\":\"Star.png\",\"parentId\":0,\"code\":\"\",\"permission\":\"AdminSchool\",\"children\":[{\"id\":250,\"code_title\":\"Báo cáo tiến độ học tập\",\"link\":\"/dashboard/progress-learn\",\"icon\":\"\",\"parentId\":25,\"code\":\"\",\"permission\":\"AdminSchool\",\"children\":null},{\"id\":251,\"code_title\":\"Báo cáo kết quả đánh giá đầu vào\",\"link\":\"/dashboard/result-pt\",\"icon\":\"\",\"parentId\":25,\"code\":\"\",\"permission\":\"AdminSchool\",\"children\":null},{\"id\":252,\"code_title\":\"Báo cáo kết quả học tập\",\"link\":\"/dashboard/result-learn\",\"icon\":\"\",\"parentId\":25,\"code\":\"\",\"permission\":\"AdminSchool\",\"children\":null},{\"id\":253,\"code_title\":\"Chuyên cần\",\"link\":\"/dashboard/diligence\",\"icon\":\"\",\"parentId\":25,\"code\":\"\",\"permission\":\"AdminSchool\",\"children\":null}]}");

            migrationBuilder.CreateIndex(
                name: "IX_Students_IsDeleted_SchoolId_SchoolClass",
                table: "Students",
                columns: new[] { "IsDeleted", "SchoolId", "SchoolClass" })
                .Annotation("SqlServer:Include", new[] { "BaseCourseLevel", "BeginnerGuideStr", "ClassCampusCode", "ClassId", "CourseId", "CourseLevel", "CreatedByParent", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "DistrictId", "ExpiredDate", "HumanId", "NumberOfShield", "Occupation", "PackageId", "ParentEmail", "ParentPhoneNumber", "ProvinceId", "School", "SchoolClassId", "SchoolFaculty", "SchoolGrade", "StudentCampusCode", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Students_IsDeleted_SchoolId_SchoolClass",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "ClassCampusCode",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "StudentCampusCode",
                table: "Students");

            migrationBuilder.UpdateData(
                table: "Menus",
                keyColumn: "Id",
                keyValue: new Guid("68365a87-73fa-4926-acb6-24f1403f77ad"),
                column: "ConfigStr",
                value: "{\"id\":25,\"code_title\":\"Dashboard\",\"link\":\"/dashboard\",\"icon\":\"Star.png\",\"parentId\":0,\"code\":\"\",\"permission\":\"AdminSchool\",\"children\":[{\"id\":250,\"code_title\":\"Báo cáo tiến độ học tập\",\"link\":\"/dashboard/progress-learn\",\"icon\":\"\",\"parentId\":25,\"code\":\"\",\"permission\":\"AdminSchool\",\"children\":null},{\"id\":251,\"code_title\":\"Báo cáo kết quả đánh giá đầu vào\",\"link\":\"/dashboard/result-pt\",\"icon\":\"\",\"parentId\":25,\"code\":\"\",\"permission\":\"AdminSchool\",\"children\":null},{\"id\":252,\"code_title\":\"Báo cáo kết quả học tập\",\"link\":\"/dashboard/result-learn\",\"icon\":\"\",\"parentId\":25,\"code\":\"\",\"permission\":\"AdminSchool\",\"children\":null},{\"id\":253,\"code_title\":\"Chuyên cần\",\"link\":\"/dashboard/diligence\",\"icon\":\"\",\"parentId\":25,\"code\":\"\",\"permission\":\"AdminSchool\",\"children\":null},{\"id\":254,\"code_title\":\"Báo cáo mục tiêu tuần\",\"link\":\"/dashboard/week-progress\",\"icon\":\"\",\"parentId\":25,\"code\":\"\",\"permission\":\"Admin,CSO,AdminSchool\",\"children\":null}]}");

            migrationBuilder.CreateIndex(
                name: "IX_Students_IsDeleted_SchoolId_SchoolClass",
                table: "Students",
                columns: new[] { "IsDeleted", "SchoolId", "SchoolClass" })
                .Annotation("SqlServer:Include", new[] { "BaseCourseLevel", "BeginnerGuideStr", "ClassId", "CourseId", "CourseLevel", "CreatedByParent", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "DistrictId", "ExpiredDate", "HumanId", "NumberOfShield", "Occupation", "PackageId", "ParentEmail", "ParentPhoneNumber", "ProvinceId", "School", "SchoolClassId", "SchoolFaculty", "SchoolGrade", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" });
        }
    }
}
