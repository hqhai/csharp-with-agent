// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Shared.Enums;

    public class VideoTimeCodeAnswerModel : BaseAnswerModel
    {
        public EnumAnswerStatus Status { get; set; }
        public Guid QuestionId { get; set; }
        public Guid VideoResultId { get; set; }
        public Guid ExerciseId { get; set; }
        public Guid VideoTimeCodeId { get; set; }
    }
}
