// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.QuestionTypeConfigs.Questions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class CheckListQuetion
    {
        public string? Name { get; set; }

        public IList<CheckListQuestionContent>? Contents { get; set; }

        public class CheckListQuestionContent
        {
            public int Id { get; set; }

            public string? Content { get; set; }

            public bool IsCorrect { get; set; }
        }
    }
}
