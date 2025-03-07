// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Shared.Enums;
    using OfficeOpenXml.Attributes;

    public class QuestionExplanationLogExportModel
    {
        [EpplusTableColumn(Header = "QuestionId")]
        public Guid QuestionId { get; set; }

        [EpplusTableColumn(Header = "QuestionType")]
        public EnumQuestionType QuestionType { get; set; }

        [EpplusTableColumn(Header = "ConfigQuestion")]
        public object? Config { get; set; }

        [EpplusTableColumn(Header = "Explanation")]
        public string? Explanation { get; set; }

        [EpplusTableColumn(Header = "PromptRequest")]
        public string? PromptRequest { get; set; }

        [EpplusTableColumn(Header = "PromptResponse")]
        public string? PromptResponse { get; set; }
    }
}
