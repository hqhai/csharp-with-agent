// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Shared.Enums;
    using OfficeOpenXml.Attributes;

    public class CourseModuleQuestionExportModel
    {
        [EpplusTableColumn(Header = "Khóa Học")]
        public EnumCourseLevel CourseLevel { get; set; }

        [EpplusTableColumn(Header = "Tống số câu hỏi dạng Standalone")]
        public long TotalQuestionStandalone { get; set; }

        [EpplusTableColumn(Header = "Tống số câu hỏi dạng SkillTest")]
        public long TotalQuestionSkillTest { get; set; }

        [EpplusTableColumn(Header = "Tống số câu hỏi dạng UnitTest")]
        public long TotalQuestionUnitTest { get; set; }

        [EpplusTableColumn(Header = "Tống số câu hỏi dạng HomeWork")]
        public long TotalQuestionHomeWork { get; set; }

        [EpplusTableColumn(Header = "Tổng số câu hỏi dạng SkillMockTest")]
        public long TotalQuestionSkillMockTest { get; set; }

        [EpplusTableColumn(Header = "Tổng số câu hỏi dạng FullMockTest")]
        public long TotalQuestionFullMockTest { get; set; }

        [EpplusTableColumn(Header = "Tổng số câu hỏi dạng FinalTest")]
        public long TotalQuestionFinalTest { get; set; }
    }
}
