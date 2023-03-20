// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.QuestionTypeConfigs.Questions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class MatchingType2Question
    {
        public string? Name { get; set; }

        public IList<MatchingType1QuestionContent>? Contents { get; set; }

        public IList<MatchingType1QuestionLink>? Links { get; set; }
    }

    public class MatchingType2QuestionContent
    {
        public int Id { get; set; }

        public string? Content { get; set; }
    }

    public class MatchingType2QuestionLink
    {
        public int FromId { get; set; }

        public int Told { get; set; }
    }
}
}
