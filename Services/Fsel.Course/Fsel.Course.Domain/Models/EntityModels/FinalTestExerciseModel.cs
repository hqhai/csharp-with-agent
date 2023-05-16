// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Fsel.Core.Base.BaseModels;

    public class FinalTestExerciseModel : BaseModel
    {
        public Guid FinalTestId { get; set; }

        public FinalTestModel? FinalTest { get; set; }

        public Guid ExerciseId { get; set; }

        public ExerciseModel? Exercise { get; set; }
    }
}
