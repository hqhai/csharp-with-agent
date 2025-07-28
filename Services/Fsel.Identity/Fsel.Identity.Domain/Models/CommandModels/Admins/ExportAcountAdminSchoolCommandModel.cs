// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.Admins
{
    using OfficeOpenXml.Attributes;

    public class ExportAcountAdminSchoolCommandModel
    {
        [EpplusTableColumn(Header = "Tên trường")]
        public string? SchoolName { get; set; }

        [EpplusTableColumn(Header = "LocalId")]
        public string? LocalId { get; set; }

        [EpplusTableColumn(Header = "Tỉnh/Thành phố")]
        public string? City { get; set; }

        [EpplusTableColumn(Header = "EventCode")]
        public string? EventCode { get; set; }

        [EpplusTableColumn(Header = "Ngày tạo tài khoản")]
        public DateTime? CreatedDate { get; set; }

        [EpplusTableColumn(Header = "Username")]
        public string? UserName { get; set; }

        [EpplusTableColumn(Header = "Mật khẩu")]
        public string? DefaultPassword { get; set; }
    }
}
