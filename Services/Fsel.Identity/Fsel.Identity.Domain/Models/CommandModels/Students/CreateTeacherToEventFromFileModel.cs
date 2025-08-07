using OfficeOpenXml.Attributes;

namespace Fsel.Identity.Domain.Models.CommandModels.Students
{
    public class CreateTeacherToEventFromFileModel
    {
        [EpplusTableColumn(Header = "Full Name\n(Họ và tên)")]
        public string? FullName { get; set; }

        [EpplusTableColumn(Header = "Phone Number\n(Số điện thoại)")]
        public string? PhoneNumber { get; set; }

        [EpplusTableColumn(Header = "Email\n(Hòm thư điện tử)")]
        public string? Email { get; set; }

        [EpplusTableColumn(Header = "Date Of Birth\n(Ngày sinh)")]
        public DateTime? DateOfBirth { get; set; }

        [EpplusTableColumn(Header = "Subject\n(Bộ môn giảng dạy)")]
        public string? Class { get; set; }

        [EpplusTableColumn(Header = "Note\n(Ghi chú)")]
        public string? Note { get; set; }
    }
}
