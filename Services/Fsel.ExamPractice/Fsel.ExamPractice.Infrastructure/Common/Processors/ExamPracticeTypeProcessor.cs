// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Infrastructure.Common.Processors
{
    using System;
    using Fsel.ExamPractice.Domain.Entities;
    using Fsel.ExamPractice.Domain.Enums;
    using Fsel.ExamPractice.Domain.Models.CommandModels.ExamPractices;

    /// <summary>
    /// Processor for ExamPractice type (Đề thi Quốc gia)
    /// </summary>
    public class ExamPracticeTypeProcessor : BaseUpdateExamPracticeProcessor
    {
        public override EnumExamPracticeType ExamPracticeType => EnumExamPracticeType.ExamPractice;

        public override void ApplyPublishRule(UpdateExamPracticeCommandModel request, ExamPractice entity)
        {
            // ExamPractice: Draft -> Active when not draft and has required fields
            if (entity.Status == EnumExamPracticeStatus.Draft)
            {
                entity.Status = EnumExamPracticeStatus.Active;
                entity.ActivatedAt = DateTime.UtcNow;
            }
        }
    }
}
