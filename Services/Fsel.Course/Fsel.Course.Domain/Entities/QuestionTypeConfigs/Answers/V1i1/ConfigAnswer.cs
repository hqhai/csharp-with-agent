// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.QuestionTypeConfigs.Answers.V1i1
{
    using System;

    public class ConfigAnswer
    {
        public Guid Id { get; set; }
        public string? Key { get; set; }
        public string? Content { get; set; }
        public bool? IsChecked { get; set; }
        public bool? IsExact { get; set; }
        public bool IsFirstSubmit { get; set; } = true;
    }
}
