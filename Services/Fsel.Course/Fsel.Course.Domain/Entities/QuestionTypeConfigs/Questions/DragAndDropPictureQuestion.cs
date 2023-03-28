// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.QuestionTypeConfigs.Questions
{
    using System.Collections.Generic;

    public class DragAndDropPictureQuestion
    {
        public string? Name { get; set; }

        public IList<DragAndDropPictureQuestionContent>? Contents { get; set; }
    }

    public class DragAndDropPictureQuestionContent
    {
        public IList<DragAndDropPictureQuestionImages>? Images { get; set; }

        public IList<DragAndDropPictureQuestionWord>? Words { get; set; }
    }

    public class DragAndDropPictureQuestionImages
    {
        public int Id { get; set; }

        public string? Path { get; set; }
    }

    public class DragAndDropPictureQuestionWord
    {
        public int Id { get; set; }
        public string? Content { get; set; }
    }
}
