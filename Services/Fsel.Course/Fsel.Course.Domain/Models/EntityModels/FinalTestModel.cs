// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Models.CommandModels.Questions;
    using Fsel.Shared.Enums;

    public class FinalTestModel : BaseModel
    {
        public string? Name { get; set; }

        public bool IsActive { get; set; }

        public int ExecutionTime { get; set; }

        public EnumCourseType CourseType { get; set; }

        public IList<ExerciseModel>? Exercises { get; set; }
    }
}
