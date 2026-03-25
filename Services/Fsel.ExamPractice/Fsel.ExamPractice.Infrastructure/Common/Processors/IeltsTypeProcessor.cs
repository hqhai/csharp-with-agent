// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Infrastructure.Common.Processors
{
    using System;
    using Fsel.ExamPractice.Domain.Entities;
    using Fsel.ExamPractice.Domain.Enums;
    using Fsel.ExamPractice.Domain.Models.CommandModels.ExamPractices;

    /// <summary>
    /// Processor for IELTS type
    /// </summary>
    public class IeltsTypeProcessor : BaseUpdateExamPracticeProcessor
    {
        public override EnumExamPracticeType ExamPracticeType => EnumExamPracticeType.IELTS;

        public override void ApplyPublishRule(UpdateExamPracticeCommandModel request, ExamPractice entity)
        {
            // IELTS: Draft -> Active (no special conditions)
            if (entity.Status == EnumExamPracticeStatus.Draft)
            {
                entity.Status = EnumExamPracticeStatus.Active;
                entity.ActivatedAt = DateTime.UtcNow;
            }
        }
    }
}
