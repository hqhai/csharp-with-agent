// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Shared.Enums;

    public class StudentBeginnerGuideModel
    {
        public IList<EnumBeginnerGuide>? BeginnerGuides { get; set; }
        public string? Other { get; set; }
        public IList<EnumQuestionType>? QuestionTypes { get; set; }
    }
}
