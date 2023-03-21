// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.QuestionTypeConfigs.Questions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class MatchingTypeQuestion
    {
        public string? Name { get; set; }

        public IList<MatchingTypeQuestionContent>? Contents { get; set; }

        public IList<MatchingTypeQuestionLink>? Links { get; set; }
    }

    public class MatchingTypeQuestionContent
    {
        public int Id { get; set; }

        public string? Content { get; set; }
    }

    public class MatchingTypeQuestionLink
    {
        public int FromId { get; set; }

        public int Told { get; set; }
    }
}
