// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.VideoTimeCodeAnswers
{
    public class CreateVideoTimeCodeAnswerCommandModel
    {
        public Guid LessonResultId { get; set; }
        public IList<VideoTimeCodeAnswerQuestionModel>? Answers { get; set; }
    }

    public class VideoTimeCodeAnswerQuestionModel
    {
        public Guid QuestionId { get; set; }
        public object? Answer { get; set; }
    }
}
