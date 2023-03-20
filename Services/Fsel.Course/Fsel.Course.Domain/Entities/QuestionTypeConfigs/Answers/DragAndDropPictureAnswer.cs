// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.QuestionTypeConfigs.Answers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class DragAndDropPictureAnswer
    {
        public int Scores { get; set; }

        public IList<DragAndDropPictureAnswers>? Answers { get; set; }
    }

    public class DragAndDropPictureAnswers
    {
        public int ImageId { get; set; }

        public int WordId { get; set; }

        public bool IsExact { get; set; }
    }
}
