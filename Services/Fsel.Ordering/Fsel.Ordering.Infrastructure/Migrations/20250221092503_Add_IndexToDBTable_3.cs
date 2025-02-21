using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Ordering.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_IndexToDBTable_3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Orders_IsDeleted_IsTrial",
                table: "Orders",
                columns: new[] { "IsDeleted", "IsTrial" })
                .Annotation("SqlServer:Include", new[] { "Address", "ClassId", "Code", "CompanyAddress", "CompanyEmail", "CompanyName", "CompanyTaxCode", "CourseId", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "DiscountPercent", "DiscountPrice", "DistrictId", "Email", "EventId", "ExpireDate", "FullName", "IsInvoice", "PackageId", "PaymentMethod", "PhoneNumber", "Price", "ProvinceId", "ReferralCode", "RevenueType", "Status", "TotalPrice", "UpdatedDate", "UpdatedFullName", "UpdatedUserId", "UserId", "VoucherId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Orders_IsDeleted_IsTrial",
                table: "Orders");
        }
    }
}
