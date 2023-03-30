// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.QuestionTypeConfigs.Questions
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    public class DragAndDropPictureQuestion
    {
        [JsonRequired]
        public string? Name { get; set; }

        [JsonRequired]
        public IList<DragAndDropPictureQuestionContent>? Contents { get; set; }
    }

    public class DragAndDropPictureQuestionContent
    {
        [JsonRequired]
        public IList<DragAndDropPictureQuestionImages>? Images { get; set; }

        [JsonRequired]
        public IList<DragAndDropPictureQuestionWord>? Words { get; set; }
    }

    public class DragAndDropPictureQuestionImages
    {
        public long Id { get; set; }

        [JsonRequired]
        public string? Path { get; set; }
    }

    public class DragAndDropPictureQuestionWord
    {
        public long Id { get; set; }

        [JsonRequired]
        public string? Content { get; set; }
    }
}
