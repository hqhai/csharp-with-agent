// Copyright (c) Atlantic. All rights reserved.

using Fsel.Course.Domain.Enums;
using Fsel.Course.Domain.Models.CommandModels.Exercises;

namespace Fsel.Course.Domain.Models.CommandModels.VideoTimeCodes
{
    public class CreateVideoTimeCodeCommandModel
    {
        public Guid? Id { get; set; }
        public EnumTimeCodeType TimeCodeType { get; set; }
        public double DisplayTime { get; set; }
        public double ExecutionTime { get; set; }
        public IList<CreateExerciseCommandModel>? Exercises { get; set; }
    }
}
