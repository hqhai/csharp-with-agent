// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Domain.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Entities;
    using Fsel.ExamPractice.Domain.Enums;
    using Fsel.ExamPractice.Domain.Models.EntityModels.ExamPracticeAnswers;

    public class ExamPracticeScore : Entity
    {
        public EnumExamPracticeScoreCriteria Criteria { get; set; }

        [MaxLength(1000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? FeedBack { get; set; }

        [NotMapped]
        public IList<ExamPracticeAIGradingLanguageModel>? GradingAlFeedbacks
        {
            get { return ConvertHelper.Deserialize<IList<ExamPracticeAIGradingLanguageModel>>(GradingAlFeedback); }
            set
            {
                if (value != null)
                {
                    GradingAlFeedback = ConvertHelper.Serialize(value);
                }
            }
        }

        public string? GradingAlFeedback { get; set; }
        public long Score { get; set; }
        public ExamPracticeSection? ExamPracticeSection { get; set; }
        public Guid ExamPracticeSectionId { get; set; }
        public ExamPracticeResult? ExamPracticeResult { get; set; }
        public Guid ExamPracticeResultId { get; set; }
    }
}
