// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using OfficeOpenXml.Attributes;

    public class ReportTimeStudentModel
    {
        [EpplusTableColumn(Header = "Student's name")]
        public string? FullName { get; set; }

        [EpplusTableColumn(Header = "Email on platform")]
        public string? Email { get; set; }

        [EpplusTableColumn(Header = "Thời gian truy cập gần nhất: Video lesson", NumberFormat = "dd/MM/yyyy HH:mm:ss")]
        public DateTime? CurrentTimeVideoLesson { get; set; }

        [EpplusTableColumn(Header = "Thời gian truy cập gần nhất: Class forum", NumberFormat = "dd/MM/yyyy HH:mm:ss")]
        public DateTime? CurrentTimeClassForum { get; set; }

        [EpplusTableColumn(Header = "Thời gian truy cập gần nhất: Homework", NumberFormat = "dd/MM/yyyy HH:mm:ss")]
        public DateTime? CurrentTimeHomework { get; set; }
    }
}
