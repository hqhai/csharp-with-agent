// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.VideoTimeCodeAnswers
{
    public class CreateVideoTimeCodeAnswerCommandModel
    {
        public Guid VideoResultId { get; set; }
        public Guid VideoTimeCodeId { get; set; }
        public bool IsSubmit { get; set; } = true;
        public IList<VideoTimeCodeAnswerQuestionModel> Answers { get; set; } = new List<VideoTimeCodeAnswerQuestionModel>();
    }

    public class VideoTimeCodeAnswerQuestionModel
    {
        public Guid QuestionId { get; set; }
        public object? Answer { get; set; }
    }
}
