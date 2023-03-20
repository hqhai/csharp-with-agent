// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.QuestionTypeConfigs.Answers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class GapFillScoreBySubQuestionAnswer
    {
        public int Scores { get; set; }

        public IList<GapFillScoreBySubQuestionAnswers>? Answers { get; set; }
    }

    public class GapFillScoreBySubQuestionAnswers
    {
        public int Id { get; set; }

        public string? Answer { get; set; }

        public bool IsExact { get; set; }
    }
}
