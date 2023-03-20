// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using System;
    using Fsel.Common.Enums;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Enums;

    public class LessonSearchModel : BaseEntityModel
    {
        public string? Name { get; set; }
        public string? TeacherName { get; set; }
        public EnumCourseLevel CourseLevel { get; set; }
        public EnumTimeCodeType TimeCodeType { get; set; }
        public bool IsActive { get; set; }
    }
}
