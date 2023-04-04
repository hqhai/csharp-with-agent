// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.QuestionTypeConfigs.Answers
{
    using System.Collections.Generic;

    public class DragAndDropPictureAnswer
    {
        public IList<DragAndDropPictureAnswers>? Answers { get; set; }
    }

    public class DragAndDropPictureAnswers
    {
        public long ImageId { get; set; }

        public long WordId { get; set; }

        public bool IsExact { get; set; }
    }
}
