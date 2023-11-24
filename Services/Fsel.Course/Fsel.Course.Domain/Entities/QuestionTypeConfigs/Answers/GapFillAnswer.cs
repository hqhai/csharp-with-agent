// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.QuestionTypeConfigs.Answers
{
    using System.Collections.Generic;

    public class GapFillAnswer
    {
        public IList<GapFillAnswers>? Answers { get; set; }
    }

    public class GapFillAnswers
    {
        public long Id { get; set; }
        public IList<string>? Answer { get; set; }
        public IList<bool?>? IsExacts { get; set; }
        public IList<GapFillAnswerExact>? GapFillExacts { get; set; }
    }

    public class GapFillAnswerExact
    {
        public bool? IsExact { get; set; }
        public bool IsExactDisplay { get; set; }
    }
}
