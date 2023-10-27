// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    public class ExerciseQuestionModel
    {
        public Guid ExerciseId { get; set; }

        public Guid QuestionId { get; set; }

        public IList<QuestionModel>? Questions { get; set; }
    }
}
