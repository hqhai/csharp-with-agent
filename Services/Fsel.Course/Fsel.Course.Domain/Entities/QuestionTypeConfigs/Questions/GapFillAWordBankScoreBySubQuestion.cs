// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.QuestionTypeConfigs.Questions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class GapFillAWordBankScoreBySubQuestion
    {
        public string? Name { get; set; }

        public IList<GapFillAWordBankScoreBySubQuestionContent>? Contents { get; set; }
    }

    public class GapFillAWordBankScoreBySubQuestionContent
    {
        public int Id { get; set; }

        public string? Content { get; set; }

        public string? Words { get; set; }
    }
}
