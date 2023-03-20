// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.QuestionTypeConfigs.Questions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class DragAndDropSentenceOrderQuestion
    {
        public string? Name { get; set; }

        public IList<DragAndDropPictureQuestion>? Contents { get; set; }
    }

    public class DragAndDropSentenceOrderQuestionContent
    {
        public int Id { get; set; }
        public string? Words { get; set; }
    }
}
