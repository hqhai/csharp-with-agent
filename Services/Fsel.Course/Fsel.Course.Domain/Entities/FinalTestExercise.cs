// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Entities;

    public class FinalTestExercise : Entity
    {
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public Guid FinalTestId { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public Guid ExerciseId { get; set; }

        public Exercise? Exercise { get; set; }
        public FinalTest? FinalTest { get; set; }
    }
}
