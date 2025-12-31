// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.VideoTimeCodeAnswers
{
    using Newtonsoft.Json;

    public class CreateVideoTimeCodeAnswerV1i1CommandModel
    {
        public Guid VideoResultId { get; set; }
        public Guid VideoTimeCodeId { get; set; }

        [JsonIgnore]
        public Guid? StudentId { get; set; }

        public bool IsTimeUp { get; set; }
        public bool IsSubmit { get; set; }
        public IList<VideoTimeCodeAnswerQuestionModel> Answers { get; set; } = new List<VideoTimeCodeAnswerQuestionModel>();
    }

    public class CreateVideoTimeCodeAnswerCommandModel
    {
        public Guid LessonId { get; set; }
        public Guid LessonResultId { get; set; }
        public IList<VideoTimeCodeAnswerQuestionModel>? Answers { get; set; }
    }

    public class VideoTimeCodeAnswerQuestionModel
    {
        public Guid QuestionId { get; set; }
        public object? Answer { get; set; }
    }
}
