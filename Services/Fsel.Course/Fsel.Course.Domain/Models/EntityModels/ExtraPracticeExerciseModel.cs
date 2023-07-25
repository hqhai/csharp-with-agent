// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;

    public class ExtraPracticeExerciseModel : BaseModel
    {
        public int TotalCount { get; set; }
        public ExerciseModel? Exercise { get; set; }
        public bool IsStatus { get; set; }
        public ExtraPracticeResultModel? ExtraPracticeExerciseResult { get; set; }
        public ExtraPracticeAnswerModel? ExtraPracticeAnswer { get; set; }
    }
}
