// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.QuestionTypeConfigs.Answers
{
    using System.Collections.Generic;

    public class GapFillAnswer
    {
        public int Scores { get; set; }

        public IList<GapFillAnswers>? Answers { get; set; }
    }

    public class GapFillAnswers
    {
        public int Id { get; set; }

        public string? Answer { get; set; }

        public bool IsExact { get; set; }
    }
}
