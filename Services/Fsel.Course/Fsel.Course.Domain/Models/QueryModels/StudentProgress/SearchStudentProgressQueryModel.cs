// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.QueryModels.StudentProgress
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class SearchStudentProgressQueryModel : BaseQueryModel
    {
        public EnumCourseType? CourseType { get; set; }
        public EnumCourseLevel? Level { get; set; }
    }
}
