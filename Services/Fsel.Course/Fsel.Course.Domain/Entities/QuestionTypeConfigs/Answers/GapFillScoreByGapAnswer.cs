// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.QuestionTypeConfigs.Answers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class GapFillScoreByGapAnswer
    {
        public int Scores { get; set; }

        public IList<GapFillScoreByGapAnswers>? Answers { get; set; }
    }

    public class GapFillScoreByGapAnswers
    {
        public int Id { get; set; }

        public string? Awswer { get; set; }

        public bool IsExact { get; set; }
    }
}
