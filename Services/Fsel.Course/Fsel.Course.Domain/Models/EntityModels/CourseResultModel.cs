// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using System;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Enums;
    using Fsel.Shared.Enums;

    public class CourseResultModel : BaseModel
    {
        public int Result { get; set; }
        public EnumCourseStatus Status { get; set; }
        public EnumCourseType? CourseType { get; set; }
        public EnumCourseLevel? CourseLevel { get; set; }
        public Guid CourseId { get; set; }
        public Guid StudentId { get; set; }
    }
}
