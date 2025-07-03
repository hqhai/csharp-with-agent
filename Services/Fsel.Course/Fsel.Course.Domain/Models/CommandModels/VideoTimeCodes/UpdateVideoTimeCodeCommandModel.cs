// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.VideoTimeCodes
{
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Models.CommandModels.Exercises;

    public class UpdateVideoTimeCodeCommandModel
    {
        public Guid? Id { get; set; }
        public EnumTimeCodeType TimeCodeType { get; set; }
        public double DisplayTime { get; set; }
        public double ExecutionTime { get; set; }
        public IList<UpdateExerciseCommandModel>? Exercises { get; set; }
    }
}
