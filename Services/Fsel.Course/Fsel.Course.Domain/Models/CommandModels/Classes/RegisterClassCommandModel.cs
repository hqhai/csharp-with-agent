// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.Classes
{
    using System;

    public class RegisterClassCommandModel
    {
        public string? Code { get; set; }
        public Guid CourseId { get; set; }
    }
}
