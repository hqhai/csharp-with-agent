// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using System;
    using Fsel.Shared.Enums;

    public class CourseResultModel : BaseResultScoreModel
    {
        public DateTime? CreatedDate { get; set; }
        public EnumCourseType? CourseType { get; set; }
        public EnumCourseLevel? CourseLevel { get; set; }
        public Guid CourseId { get; set; }
    }
}
