// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.QuestionTypeConfigs.Questions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class DropDownQuestion
    {
        public string? Name { get; set; }

        public IList<DropDownQuestionContent>? Contents { get; set; }

        public class DropDownQuestionContent
        {
            public int Id { get; set; }

            public string? Content { get; set; }

            public bool IsCorrect { get; set; }
        }
    }
}
