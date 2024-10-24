// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.Tests
{
    using OfficeOpenXml.Attributes;

    public class UpdateStudentModuleProgressCommandModel
    {
        [EpplusTableColumn(Header = "Email")]
        public string? Email { get; set; }

        [EpplusTableColumn(Header = "Loại Module")]
        public string? Type { get; set; }

        [EpplusTableColumn(Header = "Course Id")]
        public string? CourseId { get; set; }

        [EpplusTableColumn(Header = "Unit Id")]
        public string? UnitId { get; set; }

        [EpplusTableColumn(Header = "Lesson Id")]
        public string? LessonId { get; set; }

        [EpplusTableColumn(Header = "Object Id")]
        public string? ObjectId { get; set; }
    }
}
