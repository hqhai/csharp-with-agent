// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Infrastructure.Common.Processors
{
    using System;
    using Fsel.ExamPractice.Domain.Entities;
    using Fsel.ExamPractice.Domain.Enums;
    using Fsel.ExamPractice.Domain.Models.CommandModels.ExamPractices;

    /// <summary>
    /// Processor for Vstep type (Bộ đề Vstep)
    /// </summary>
    public class VstepTypeProcessor : BaseUpdateExamPracticeProcessor
    {
        public override EnumExamPracticeType ExamPracticeType => EnumExamPracticeType.Vstep;

        public override void ApplyPublishRule(UpdateExamPracticeCommandModel request, ExamPractice entity)
        {
            // Vstep: Draft -> Active (no special conditions)
            if (entity.Status == EnumExamPracticeStatus.Draft)
            {
                entity.Status = EnumExamPracticeStatus.Active;
                entity.ActivatedAt = DateTime.UtcNow;
            }
        }
    }
}
