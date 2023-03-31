// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.QuestionTypeConfigs.Answers
{
    public class ListingAnswer
    {
        public IList<string>? Answers { get; set; }

        public bool IsExact { get; set; }
    }
}
