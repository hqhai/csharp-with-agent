// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Domain.Entities.QuestionTypeConfigs.Answers.V1i1
{
    using System;
    using Fsel.Shared.Enums;

    public class ConfigAnswer
    {
        public Guid Id { get; set; }
        public string? Key { get; set; }
        public string? Content { get; set; }
        public bool? IsExact { get; set; }
        public bool IsFirstSubmit { get; set; } = true;

        public EnumCorrectStatus Status
        {
            get
            {
                return IsExact.HasValue ? IsExact.Value ? EnumCorrectStatus.Correct : EnumCorrectStatus.Fail : EnumCorrectStatus.Process;
            }
        }
    }
}
