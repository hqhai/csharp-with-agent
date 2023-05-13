// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.FinalTests
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Models.CommandModels.Exercises;
    using Fsel.Shared.Enums;

    public class CreateFinalTestCommandModel
    {
        public string? Name { get; set; }

        public bool IsActive { get; set; }

        [Range(0, 10000_0000, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public int ExecutionTime { get; set; }

        public EnumCourseType CourseType { get; set; }

        public IList<CreateExerciseCommandModel>? Exercises { get; set; }
    }
}
