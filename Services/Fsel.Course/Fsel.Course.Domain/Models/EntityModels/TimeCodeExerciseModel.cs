// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    public class TimeCodeExerciseModel
    {
        public Guid ExerciseId { get; set; }

        public IList<ExerciseModel>? Exercices { get; set; }

        public Guid VideoTimeCodeId { get; set; }
    }
}
