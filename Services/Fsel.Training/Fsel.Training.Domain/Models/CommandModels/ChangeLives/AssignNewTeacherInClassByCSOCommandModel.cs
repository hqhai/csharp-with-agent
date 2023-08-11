// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Domain.Models.CommandModels.ChangeLives
{
    using System;

    public class AssignNewTeacherInClassByCSOCommandModel
    {
        public Guid ClassId { get; set; }
        public Guid TeacherId { get; set; }
    }
}
