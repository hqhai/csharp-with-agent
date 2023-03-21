// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.QuestionTypeConfigs.Answers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class ListingAnswer
    {
        public string? Answers { get; set; }

        public bool IsExact { get; set; }

        public int Scores { get; set; }
    }
}
