// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.QuestionTypeConfigs.Answers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class DragAndDropSentenceOrderAnswer
    {
        public int Score { get; set; }

        public IList<DragAndDropSentenceOrderAnswers>? Answers { get; set; }
    }

    public class DragAndDropSentenceOrderAnswers
    {
        public int Id { get; set; }

        public string? Answer { get; set; }

        public bool IsExact { get; set; }
    }
}
