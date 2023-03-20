// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.QuestionTypeConfigs.Answers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class MatchingType2Answer
    {
        public IList<MatchingType2Answers>? Answers { get; set; }

        public int Scores { get; set; }
    }

    public class MatchingType2Answers
    {
        public int FromId { get; set; }

        public int Told { get; set; }

        public bool IsExact { get; set; }
    }
}
