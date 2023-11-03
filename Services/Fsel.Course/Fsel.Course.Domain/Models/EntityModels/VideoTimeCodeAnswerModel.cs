// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    public class VideoTimeCodeAnswerModel : BaseAnswerModel
    {
        public Guid QuestionId { get; set; }
        public Guid VideoResultId { get; set; }
        public Guid ExerciseId { get; set; }
        public Guid VideoTimeCodeId { get; set; }
    }
}
