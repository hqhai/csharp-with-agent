using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.System.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_IndexToDBTable_4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TokenHistories_IsDeleted_UserId_CreatedDate",
                table: "TokenHistories");

            migrationBuilder.DropIndex(
                name: "IX_TokenHistories_IsDeleted_UserId_Type",
                table: "TokenHistories");

            migrationBuilder.CreateIndex(
                name: "IX_TokenHistories_IsDeleted_UserId",
                table: "TokenHistories",
                columns: new[] { "IsDeleted", "UserId" })
                .Annotation("SqlServer:Include", new[] { "VolatileToken", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_TokenHistories_IsDeleted_UserId_CreatedDate",
                table: "TokenHistories",
                columns: new[] { "IsDeleted", "UserId", "CreatedDate" })
                .Annotation("SqlServer:Include", new[] { "ConfigDataStr", "ConfigStr", "CourseResultId", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "Feature", "InitialToken", "Mission", "ObjectId", "RemainToken", "TokenConfigId", "Type", "UpdatedDate", "UpdatedFullName", "UpdatedUserId", "VolatileToken" });

            migrationBuilder.CreateIndex(
                name: "IX_TokenHistories_IsDeleted_UserId_Type",
                table: "TokenHistories",
                columns: new[] { "IsDeleted", "UserId", "Type" })
                .Annotation("SqlServer:Include", new[] { "ConfigDataStr", "ConfigStr", "CourseResultId", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "Feature", "InitialToken", "Mission", "ObjectId", "RemainToken", "TokenConfigId", "UpdatedDate", "UpdatedFullName", "UpdatedUserId", "VolatileToken" });

            migrationBuilder.CreateIndex(
                name: "IX_BannerStudents_IsDeleted_StudentId_CreatedDate",
                table: "BannerStudents",
                columns: new[] { "IsDeleted", "StudentId", "CreatedDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TokenHistories_IsDeleted_UserId",
                table: "TokenHistories");

            migrationBuilder.DropIndex(
                name: "IX_TokenHistories_IsDeleted_UserId_CreatedDate",
                table: "TokenHistories");

            migrationBuilder.DropIndex(
                name: "IX_TokenHistories_IsDeleted_UserId_Type",
                table: "TokenHistories");

            migrationBuilder.DropIndex(
                name: "IX_BannerStudents_IsDeleted_StudentId_CreatedDate",
                table: "BannerStudents");

            migrationBuilder.CreateIndex(
                name: "IX_TokenHistories_IsDeleted_UserId_CreatedDate",
                table: "TokenHistories",
                columns: new[] { "IsDeleted", "UserId", "CreatedDate" });

            migrationBuilder.CreateIndex(
                name: "IX_TokenHistories_IsDeleted_UserId_Type",
                table: "TokenHistories",
                columns: new[] { "IsDeleted", "UserId", "Type" });
        }
    }
}
