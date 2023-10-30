// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    public class VideoTimeCodeResultModel : BaseResultScoreModel
    {
        public int TotalCount { get; set; }
        public bool Ungraded { get; set; }
        public Guid VideoId { get; set; }
        public double DisplayTime { get; set; }
        public double ExecutionTime { get; set; }
        public IList<ExerciseModel>? Exercises { get; set; }
    }
}
