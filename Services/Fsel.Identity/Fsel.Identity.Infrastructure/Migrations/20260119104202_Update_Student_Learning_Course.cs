using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Identity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_Student_Learning_Course : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Students_IsDeleted_SchoolId_SchoolClass",
                table: "Students");

            migrationBuilder.AddColumn<Guid>(
                name: "LevelId",
                table: "Students",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProgramId",
                table: "Students",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SubjectId",
                table: "Students",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Students_IsDeleted_SchoolId_SchoolClass",
                table: "Students",
                columns: new[] { "IsDeleted", "SchoolId", "SchoolClass" })
                .Annotation("SqlServer:Include", new[] { "BaseCourseLevel", "BeginnerGuideStr", "ClassCampusCode", "ClassId", "CourseId", "CourseLevel", "CreatedByParent", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "DistrictId", "ExpiredDate", "LevelId", "NumberOfShield", "Occupation", "PackageId", "ParentEmail", "ParentPhoneNumber", "ProgramId", "ProvinceId", "School", "SchoolClassId", "SchoolFaculty", "SchoolGrade", "StatusStudentCampus", "StudentCampusCode", "SubjectId", "UpdatedDate", "UpdatedFullName", "UpdatedUserId", "UserId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Students_IsDeleted_SchoolId_SchoolClass",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "LevelId",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "ProgramId",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "SubjectId",
                table: "Students");

            migrationBuilder.CreateIndex(
                name: "IX_Students_IsDeleted_SchoolId_SchoolClass",
                table: "Students",
                columns: new[] { "IsDeleted", "SchoolId", "SchoolClass" })
                .Annotation("SqlServer:Include", new[] { "BaseCourseLevel", "BeginnerGuideStr", "ClassCampusCode", "ClassId", "CourseId", "CourseLevel", "CreatedByParent", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "DistrictId", "ExpiredDate", "NumberOfShield", "Occupation", "PackageId", "ParentEmail", "ParentPhoneNumber", "ProvinceId", "School", "SchoolClassId", "SchoolFaculty", "SchoolGrade", "StatusStudentCampus", "StudentCampusCode", "UpdatedDate", "UpdatedFullName", "UpdatedUserId", "UserId" });
        }
    }
}
