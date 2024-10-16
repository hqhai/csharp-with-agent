// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using OfficeOpenXml.Attributes;

    public class ImportQuestionIdModel
    {
        [EpplusTableColumn(Header = "QuestionId")]
        public string? QuestionId { get; set; }
    }
}
