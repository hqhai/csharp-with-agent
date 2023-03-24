// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using System;
    using Fsel.Core.Base.BaseModels;

    public class CourseTeacherModel : BaseModel
    {
        public Guid TeacherId { get; set; }
        public Guid CourseId { get; set; }
    }
}
