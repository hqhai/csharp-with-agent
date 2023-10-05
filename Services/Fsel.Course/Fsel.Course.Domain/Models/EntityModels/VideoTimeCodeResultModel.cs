// Copyright (c) Atlantic. All rights reserved.

using Fsel.Course.Domain.Enums;
using Fsel.Shared.Enums;

namespace Fsel.Course.Domain.Models.EntityModels
{
    public class VideoTimeCodeResultModel : BaseResultScoreModel
    {
        public int TotalCount { get; set; }
        public bool Ungraded { get; set; }
        public EnumTimeCodeType TimeCodeType { get; set; }
        public EnumTimeCodeStatus TimeCodeStatus { get; set; }
        public Guid VideoId { get; set; }
        public double DisplayTime { get; set; }
        public double ExecutionTime { get; set; }
        public IList<ExerciseModel>? Exercises { get; set; }
    }
}
