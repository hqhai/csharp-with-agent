// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using System;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;

    public class HomeWorkConfigModel : BaseModel
    {
        public string? HomeWorkName { get; set; }
        public int NumberRetry { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public Guid HomeWorkId { get; set; }
        public Guid CurriculumId { get; set; }
        public EnumCourseLevel? CourseLevel { get; set; }
        public EnumCourseSkill? CourseSkill { get; set; }

        public EnumCourseType CourseType
        {
            get { return EnumCourseLevelHelper.GetEnumCourseType(CourseLevel); }
        }
    }
}
