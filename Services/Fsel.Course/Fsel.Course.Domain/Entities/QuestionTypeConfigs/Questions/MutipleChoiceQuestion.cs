// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.QuestionTypeConfigs.Questions
{
    using System.Collections.Generic;

    public class MutipleChoiceQuestion
    {
        public string? Name { get; set; }

        public IList<MutipleChoiceQuestionContent>? Contents { get; set; }
    }

    public class MutipleChoiceQuestionContent
    {
        public int Id { get; set; }

        public string? Content { get; set; }

        public bool? IsCorrect { get; set; }
    }
}
