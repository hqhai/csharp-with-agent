// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Domain.Models.EntityModels.ExamPractices
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.ExamPractice.Domain.Enums;

    public class ExamPracticeReportModel : BaseModel
    {
        public string? Name { get; set; }
        public EnumExamPracticeType Type { get; set; }
        public EnumExamPracticeSubType SubType { get; set; }
        public ExamPracticeResultModel? ExamPracticeResult { get; set; }
    }
}
