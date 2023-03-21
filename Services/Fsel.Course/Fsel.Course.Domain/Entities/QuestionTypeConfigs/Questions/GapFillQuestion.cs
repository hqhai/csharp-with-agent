// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.QuestionTypeConfigs.Questions
{
    using System.Collections.Generic;

    public class GapFillQuestion
    {
        public string? Name { get; set; }

        public IList<GapFillScoreBySubQuesionContent>? Contents { get; set; }
    }

    public class GapFillScoreBySubQuesionContent
    {
        public int Id { get; set; }

        public string? Content { get; set; }

        public string? Words { get; set; }
    }
}
