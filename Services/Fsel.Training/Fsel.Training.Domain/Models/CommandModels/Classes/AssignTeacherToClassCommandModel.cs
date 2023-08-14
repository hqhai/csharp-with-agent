// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Domain.Models.CommandModels.Classes
{
    using System;

    public class AssignTeacherToClassCommandModel
    {
        public Guid ClassId { get; set; }
        public Guid TeacherId { get; set; }
    }
}
