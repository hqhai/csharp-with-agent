// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using System.Collections.Generic;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class FinalTestSearchModel : BaseModel
    {
        public string? Name { get; set; }

        public bool IsActive { get; set; }

        public double ExecutionTime { get; set; }

        public EnumFinalTestLevel FinalTestLevel { get; set; }

        public IList<ExerciseModel>? Exercises { get; set; }
    }
}
