// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.Tests
{
    using System;
    using System.Collections.Generic;
    using Fsel.Shared.Enums;

    public class CreateTestAnswerCommandModel
    {
        public EnumPlacementTestLevel Level { get; set; }
        public double TotalQuestion { get; set; }
        public double CountQuestion { get; set; }
        public IList<TestAnswerSkillQuestionModel>? Skills { get; set; }
    }

    public class TestAnswerQuestionModel
    {
        public Guid QuestionId { get; set; }
        public object? Answer { get; set; }
    }

    public class TestAnswerSkillQuestionModel
    {
        public EnumCourseSkill Skill { get; set; }
        public IList<TestAnswerQuestionModel>? Answers { get; set; }
    }
}
