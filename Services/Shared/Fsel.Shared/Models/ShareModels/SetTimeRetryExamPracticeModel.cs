// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.ShareModels
{
    public class SetTimeRetryExamPracticeModel : ChatGptConfigModel
    {
        public Guid ExamPracticeSectionId { get; set; }
        public Guid ExamPracticeSectionResultId { get; set; }
        public string? WordContent { get; set; }
        public bool IsRetry { get; set; }
        public DateTime StartDate { get; set; }
    }
}
