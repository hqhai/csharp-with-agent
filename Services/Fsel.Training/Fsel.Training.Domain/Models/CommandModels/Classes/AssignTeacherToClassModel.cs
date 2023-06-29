// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Domain.Models.CommandModels.Classes
{
    using System;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Training.Domain.Enums;

    public class AssignTeacherToClassModel : BaseCommandModel
    {
        public Guid TeacherId { get; set; }
    }
}
