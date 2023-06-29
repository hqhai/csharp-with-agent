// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Domain.Models.CommandModels.Classes
{
    using System;

    public class AssignTeacherToClassModel
    {
        public Guid Id { get; set; }
        public Guid TeacherId { get; set; }
    }
}
