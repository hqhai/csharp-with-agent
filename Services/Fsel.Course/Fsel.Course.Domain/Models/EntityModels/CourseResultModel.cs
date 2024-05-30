// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using System;
    using Fsel.Course.Domain.IEntities;
    using Fsel.Shared.Enums;

    public class CourseResultModel : BaseScoreResultModel, IModuleLifeCycle
    {
        public EnumCourseType? CourseType { get; set; }
        public EnumWorkingStatus WorkingStatus { get; set; }
        public EnumCourseLevel? CourseLevel { get; set; }
        public DateTime? ProcessDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        public Guid CourseId { get; set; }
    }
}
