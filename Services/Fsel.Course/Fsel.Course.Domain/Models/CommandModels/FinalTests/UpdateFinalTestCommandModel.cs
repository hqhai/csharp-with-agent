// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.FinalTests
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Models.CommandModels.Exercises;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;

    public class UpdateFinalTestCommandModel : BaseCommandModel
    {
        public string? Name { get; set; }

        public bool IsActive { get; set; }

        [Range(0, 10000_0000, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public int ExecutionTime { get; set; }

        public EnumFinalTestLevel FinalTestLevel { get; set; }

        public IList<UpdateExerciseCommandModel>? Exercises { get; set; }
    }
}
