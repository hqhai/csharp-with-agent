using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Identity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_IndexToDBTable_4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "RefreshToken",
                table: "AspNetUserTokens",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Students_IsDeleted_SchoolId_SchoolClass",
                table: "Students",
                columns: new[] { "IsDeleted", "SchoolId", "SchoolClass" })
                .Annotation("SqlServer:Include", new[] { "BaseCourseLevel", "BeginnerGuideStr", "ClassId", "CourseId", "CourseLevel", "CreatedByParent", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "DistrictId", "ExpiredDate", "HumanId", "NumberOfShield", "Occupation", "PackageId", "ParentEmail", "ParentPhoneNumber", "ProvinceId", "School", "SchoolFaculty", "SchoolGrade", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserTokens_IsDeleted_RefreshToken",
                table: "AspNetUserTokens",
                columns: new[] { "IsDeleted", "RefreshToken" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Students_IsDeleted_SchoolId_SchoolClass",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUserTokens_IsDeleted_RefreshToken",
                table: "AspNetUserTokens");

            migrationBuilder.AlterColumn<string>(
                name: "RefreshToken",
                table: "AspNetUserTokens",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);
        }
    }
}
