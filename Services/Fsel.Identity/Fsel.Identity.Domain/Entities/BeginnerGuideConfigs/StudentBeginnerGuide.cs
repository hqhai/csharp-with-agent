// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Entities.BeginnerGuideConfigs
{
    using Fsel.Shared.Enums;

    public class StudentBeginnerGuide
    {
        public IList<EnumBeginnerGuide>? BeginnerGuides { get; set; }
        public string? Other { get; set; }
        public IList<EnumQuestionType>? QuestionTypes { get; set; }
    }
}
