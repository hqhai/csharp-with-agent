using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Identity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialOpenIdDbMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CSOs_Humans_HumanId",
                table: "CSOs");

            migrationBuilder.DropForeignKey(
                name: "FK_Parents_Humans_HumanId",
                table: "Parents");

            migrationBuilder.DropForeignKey(
                name: "FK_Students_Humans_HumanId",
                table: "Students");

            migrationBuilder.DropForeignKey(
                name: "FK_Teachers_Humans_HumanId",
                table: "Teachers");

            migrationBuilder.DropIndex(
                name: "IX_Students_IsDeleted",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Students_IsDeleted_SchoolId_SchoolClass",
                table: "Students");

            migrationBuilder.RenameColumn(
                name: "OTPCode",
                table: "UserOtpCodes",
                newName: "OtpCode");

            migrationBuilder.RenameIndex(
                name: "IX_UserOtpCodes_UserId_OTPCode",
                table: "UserOtpCodes",
                newName: "IX_UserOtpCodes_UserId_OtpCode");

            migrationBuilder.RenameIndex(
                name: "IX_UserOtpCodes_IsDeleted_OTPCode_Status",
                table: "UserOtpCodes",
                newName: "IX_UserOtpCodes_IsDeleted_OtpCode_Status");

            migrationBuilder.RenameColumn(
                name: "HumanId",
                table: "Teachers",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Teachers_HumanId",
                table: "Teachers",
                newName: "IX_Teachers_UserId");

            migrationBuilder.RenameColumn(
                name: "HumanId",
                table: "Students",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Students_HumanId",
                table: "Students",
                newName: "IX_Students_UserId");

            migrationBuilder.RenameColumn(
                name: "HumanId",
                table: "Parents",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Parents_HumanId",
                table: "Parents",
                newName: "IX_Parents_UserId");

            migrationBuilder.RenameColumn(
                name: "HumanId",
                table: "CSOs",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_CSOs_HumanId",
                table: "CSOs",
                newName: "IX_CSOs_UserId");

            migrationBuilder.RenameColumn(
                name: "FullName",
                table: "AspNetUsers",
                newName: "LastName");

            migrationBuilder.AlterColumn<string>(
                name: "OtpCode",
                table: "UserOtpCodes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "VerifyId",
                table: "UserOtpCodes",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "AspNetUsers",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AvatarPath",
                table: "AspNetUsers",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Birthday",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "AspNetUsers",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "AspNetUsers",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Gender",
                table: "AspNetUsers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DataProtectionKeys",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FriendlyName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Xml = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataProtectionKeys", x => x.Id);
                });

            // Update dữ liệu từ Humans -> AspNetUsers, và xóa HumanId ở các bảng liên quan
            migrationBuilder.Sql(@"
                UPDATE U
                SET 
                    Address     = ISNULL(H.Address, U.Address),
                    AvatarPath  = ISNULL(H.AvatarPath, U.AvatarPath),
                    Birthday    = ISNULL(H.Birthday, U.Birthday),
                    Code        = ISNULL(H.Code, U.Code),
                    PhoneNumber = ISNULL(H.PhoneNumber, U.PhoneNumber),
                    Gender      = ISNULL(H.Gender, U.Gender),
                    LastName    = TRIM(LEFT(ISNULL(H.FullName, U.LastName), CHARINDEX(' ', ISNULL(H.FullName, U.LastName)))),
                    FirstName   = TRIM(STUFF(ISNULL(H.FullName, U.LastName), 1, CHARINDEX(' ', ISNULL(H.FullName, U.LastName)), ''))
                FROM AspNetUsers U
                LEFT JOIN Humans H ON U.Id = H.UserId;

                UPDATE CSOs
                SET UserId = U.Id
                FROM CSOs C
                JOIN Humans H ON C.HumanId = H.Id
                JOIN AspNetUsers U ON H.UserId = U.Id;

                UPDATE Parents
                SET UserId = U.Id
                FROM Parents P
                JOIN Humans H ON P.HumanId = H.Id
                JOIN AspNetUsers U ON H.UserId = U.Id;

                DELETE FROM ParentStudents WHERE ParentId IN (SELECT Id FROM Parents WHERE UserId IS NULL);
                DELETE FROM Parents WHERE UserId IS NULL;

                UPDATE Students
                SET UserId = U.Id
                FROM Students S
                JOIN Humans H ON S.HumanId = H.Id
                JOIN AspNetUsers U ON H.UserId = U.Id;

                DELETE FROM ParentStudents WHERE StudentId IN (SELECT Id FROM Students WHERE UserId IS NULL);
                DELETE FROM Students WHERE UserId IS NULL;

                UPDATE Teachers
                SET UserId = U.Id
                FROM Teachers T
                JOIN Humans H ON T.HumanId = H.Id
                JOIN AspNetUsers U ON H.UserId = U.Id;
            ");

            migrationBuilder.DropTable(
                name: "Humans");

            migrationBuilder.CreateIndex(
                name: "IX_Students_IsDeleted",
                table: "Students",
                column: "IsDeleted")
                .Annotation("SqlServer:Include", new[] { "CreatedDate", "School", "CourseLevel", "SchoolId" });

            migrationBuilder.CreateIndex(
                name: "IX_Students_IsDeleted_SchoolId_SchoolClass",
                table: "Students",
                columns: new[] { "IsDeleted", "SchoolId", "SchoolClass" })
                .Annotation("SqlServer:Include", new[] { "BaseCourseLevel", "BeginnerGuideStr", "ClassId", "CourseId", "CourseLevel", "CreatedByParent", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "DistrictId", "ExpiredDate", "NumberOfShield", "Occupation", "PackageId", "ParentEmail", "ParentPhoneNumber", "ProvinceId", "School", "SchoolFaculty", "SchoolGrade", "UpdatedDate", "UpdatedFullName", "UpdatedUserId", "UserId" });

            migrationBuilder.AddForeignKey(
                name: "FK_CSOs_AspNetUsers_UserId",
                table: "CSOs",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Parents_AspNetUsers_UserId",
                table: "Parents",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Students_AspNetUsers_UserId",
                table: "Students",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Teachers_AspNetUsers_UserId",
                table: "Teachers",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CSOs_AspNetUsers_UserId",
                table: "CSOs");

            migrationBuilder.DropForeignKey(
                name: "FK_Parents_AspNetUsers_UserId",
                table: "Parents");

            migrationBuilder.DropForeignKey(
                name: "FK_Students_AspNetUsers_UserId",
                table: "Students");

            migrationBuilder.DropForeignKey(
                name: "FK_Teachers_AspNetUsers_UserId",
                table: "Teachers");

            migrationBuilder.DropTable(
                name: "DataProtectionKeys");

            migrationBuilder.DropIndex(
                name: "IX_Students_IsDeleted",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Students_IsDeleted_SchoolId_SchoolClass",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "VerifyId",
                table: "UserOtpCodes");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "AvatarPath",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Birthday",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "OtpCode",
                table: "UserOtpCodes",
                newName: "OTPCode");

            migrationBuilder.RenameIndex(
                name: "IX_UserOtpCodes_UserId_OtpCode",
                table: "UserOtpCodes",
                newName: "IX_UserOtpCodes_UserId_OTPCode");

            migrationBuilder.RenameIndex(
                name: "IX_UserOtpCodes_IsDeleted_OtpCode_Status",
                table: "UserOtpCodes",
                newName: "IX_UserOtpCodes_IsDeleted_OTPCode_Status");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Teachers",
                newName: "HumanId");

            migrationBuilder.RenameIndex(
                name: "IX_Teachers_UserId",
                table: "Teachers",
                newName: "IX_Teachers_HumanId");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Students",
                newName: "HumanId");

            migrationBuilder.RenameIndex(
                name: "IX_Students_UserId",
                table: "Students",
                newName: "IX_Students_HumanId");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Parents",
                newName: "HumanId");

            migrationBuilder.RenameIndex(
                name: "IX_Parents_UserId",
                table: "Parents",
                newName: "IX_Parents_HumanId");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "CSOs",
                newName: "HumanId");

            migrationBuilder.RenameIndex(
                name: "IX_CSOs_UserId",
                table: "CSOs",
                newName: "IX_CSOs_HumanId");

            migrationBuilder.RenameColumn(
                name: "LastName",
                table: "AspNetUsers",
                newName: "FullName");

            migrationBuilder.AlterColumn<string>(
                name: "OTPCode",
                table: "UserOtpCodes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.CreateTable(
                name: "Humans",
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
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    AvatarPath = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Birthday = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Code = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(70)", maxLength: 70, nullable: true),
                    FullName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Humans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Humans_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Rollback dữ liệu từ AspNetUsers -> Humans, và khôi phục HumanId ở các bảng liên quan
            migrationBuilder.Sql(@"
                INSERT INTO Humans (Id, CreatedUserId, CreatedFullName, CreatedDate, IsDeleted, UserId, Address, AvatarPath, Birthday, Code, FullName, Gender, PhoneNumber)
                SELECT NEWID(), '00000000-0000-0000-0000-000000000000', 'MigrationRollback', GETDATE(), 0, U.Id, U.Address, U.AvatarPath, U.Birthday, U.Code, 
                   CONCAT(U.FirstName, ' ', U.FullName), U.Gender, U.PhoneNumber
                FROM AspNetUsers U;
                
                UPDATE C
                SET HumanId = H.Id
                FROM CSOs C
                JOIN AspNetUsers U ON C.UserId = U.Id
                JOIN Humans H ON H.UserId = U.Id;
                
                UPDATE P
                SET HumanId = H.Id
                FROM Parents P
                JOIN AspNetUsers U ON P.UserId = U.Id
                JOIN Humans H ON H.UserId = U.Id;
                
                UPDATE S
                SET HumanId = H.Id
                FROM Students S
                JOIN AspNetUsers U ON S.UserId = U.Id
                JOIN Humans H ON H.UserId = U.Id;
                
                UPDATE T
                SET HumanId = H.Id
                FROM Teachers T
                JOIN AspNetUsers U ON T.UserId = U.Id
                JOIN Humans H ON H.UserId = U.Id;
            ");

            migrationBuilder.CreateIndex(
                name: "IX_Students_IsDeleted",
                table: "Students",
                column: "IsDeleted")
                .Annotation("SqlServer:Include", new[] { "CreatedDate", "School", "CourseLevel", "HumanId", "SchoolId" });

            migrationBuilder.CreateIndex(
                name: "IX_Students_IsDeleted_SchoolId_SchoolClass",
                table: "Students",
                columns: new[] { "IsDeleted", "SchoolId", "SchoolClass" })
                .Annotation("SqlServer:Include", new[] { "BaseCourseLevel", "BeginnerGuideStr", "ClassId", "CourseId", "CourseLevel", "CreatedByParent", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "DistrictId", "ExpiredDate", "HumanId", "NumberOfShield", "Occupation", "PackageId", "ParentEmail", "ParentPhoneNumber", "ProvinceId", "School", "SchoolFaculty", "SchoolGrade", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" });

            migrationBuilder.CreateIndex(
                name: "IX_Humans_IsDeleted_Code",
                table: "Humans",
                columns: new[] { "IsDeleted", "Code" });

            migrationBuilder.CreateIndex(
                name: "IX_Humans_IsDeleted_Email",
                table: "Humans",
                columns: new[] { "IsDeleted", "Email" });

            migrationBuilder.CreateIndex(
                name: "IX_Humans_IsDeleted_PhoneNumber",
                table: "Humans",
                columns: new[] { "IsDeleted", "PhoneNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_Humans_UserId",
                table: "Humans",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_CSOs_Humans_HumanId",
                table: "CSOs",
                column: "HumanId",
                principalTable: "Humans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Parents_Humans_HumanId",
                table: "Parents",
                column: "HumanId",
                principalTable: "Humans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Students_Humans_HumanId",
                table: "Students",
                column: "HumanId",
                principalTable: "Humans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Teachers_Humans_HumanId",
                table: "Teachers",
                column: "HumanId",
                principalTable: "Humans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
