// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using System;
    using Fsel.Core.Base.BaseModels;

    public class ExtraPracticeExerciseModel : BaseModel
    {
        public Guid ExtraPracticeChapterId { get; set; }
        public Guid ExtraPracticeId { get; set; }
        public Guid ExerciseId { get; set; }
    }
}
