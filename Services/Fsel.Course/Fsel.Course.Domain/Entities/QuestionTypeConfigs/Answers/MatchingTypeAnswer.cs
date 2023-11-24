// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.QuestionTypeConfigs.Answers
{
    using System.Collections.Generic;

    public class MatchingTypeAnswer
    {
        public IList<MatchingTypeAnswers>? Answers { get; set; }
    }

    public class MatchingTypeAnswers
    {
        public long? FromId { get; set; }
        public long? ToId { get; set; }
        public bool? IsExact { get; set; }
        public bool? IsExactDisplay { get; set; }
    }
}
