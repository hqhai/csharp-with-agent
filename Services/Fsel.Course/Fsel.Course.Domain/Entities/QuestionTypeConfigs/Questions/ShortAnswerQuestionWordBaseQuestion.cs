// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.QuestionTypeConfigs.Questions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class ShortAnswerQuestionWordBaseQuestion
    {
        public string? Name { get; set; }

        public string? Content { get; set; }

        public bool IsSpeakRequired { get; set; }
    }
}
