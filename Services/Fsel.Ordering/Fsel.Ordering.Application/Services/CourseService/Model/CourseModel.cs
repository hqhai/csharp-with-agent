// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Services.CourseService.Model
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class CourseModel : BaseModel
    {
        public string? Name { get; set; }
        public EnumCourseLevel CourseLevel { get; set; }
    }
}
