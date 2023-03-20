// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.QuestionTypeConfigs.Answers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class GapFillAWordBankScoreByGapAnswer
    {
        public int Scores { get; set; }

        public IList<GapFillAWordBankScoreByGapAnswers>? Answers { get; set; }
    }

    public class GapFillAWordBankScoreByGapAnswers
    {
        public int Id { get; set; }

        public string? Awswer { get; set; }

        public bool IsExact { get; set; }
    }
}
