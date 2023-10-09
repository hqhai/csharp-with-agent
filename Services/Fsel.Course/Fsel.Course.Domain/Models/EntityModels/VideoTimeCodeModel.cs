// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Base.BaseModels;
using Fsel.Course.Domain.Enums;
using Fsel.Shared.Enums;

namespace Fsel.Course.Domain.Models.EntityModels
{
    public class VideoTimeCodeModel : BaseModel
    {
        public int TotalCount { get; set; }
        public bool Ungraded { get; set; }
        public int CorrectTotal { get; set; }
        public int CorrectCount { get; set; }
        public EnumTimeCodeType TimeCodeType { get; set; }
        public EnumTimeCodeStatus Status { get; set; }
        public Guid VideoId { get; set; }
        public double DisplayTime { get; set; }
        public double ExecutionTime { get; set; }
        public long WorkingTime { get; set; }
        public IList<ExerciseModel>? Exercises { get; set; }
    }
}
