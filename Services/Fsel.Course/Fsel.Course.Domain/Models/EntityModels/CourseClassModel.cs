// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using System;
    using Fsel.Core.Base.BaseModels;

    public class CourseClassModel : BaseModel
    {
        public Guid ClassId { get; set; }
        public Guid CourseId { get; set; }
    }
}
