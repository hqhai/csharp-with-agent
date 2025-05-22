// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.UserServices.QueryModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class SearchStudentsQueryModel : BaseQueryModel
    {
        public bool IsCourseProcess { get; set; }
        public EnumCourseType? CourseType { get; set; }
        public EnumCourseLevel? CourseLevel { get; set; }
    }
}
