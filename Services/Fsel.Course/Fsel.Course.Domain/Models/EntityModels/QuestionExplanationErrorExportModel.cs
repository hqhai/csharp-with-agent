// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Shared.Enums;
    using OfficeOpenXml.Attributes;

    public class QuestionExplanationErrorExportModel
    {
        [EpplusTableColumn(Header = "VideoId")]
        public Guid VideoId { get; set; }

        [EpplusTableColumn(Header = "CourseLevel")]
        public EnumCourseLevel CourseLevel { get; set; }

        [EpplusTableColumn(Header = "DisplayTime")]
        public double DisplayTime { get; set; }

        [EpplusTableColumn(Header = "QuestionId")]
        public Guid QuestionId { get; set; }

        [EpplusTableColumn(Header = "QuestionType")]
        public EnumQuestionType QuestionType { get; set; }

        [EpplusTableColumn(Header = "ConfigQuestion")]
        public object? Config { get; set; }

        [EpplusTableColumn(Header = "Explanation")]
        public string? Explanation { get; set; }

        [EpplusTableColumn(Header = "User Feedback")]
        public string? Feedback { get; set; }
    }
}
