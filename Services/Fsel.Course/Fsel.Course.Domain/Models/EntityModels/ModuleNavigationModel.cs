// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using System;
    using Fsel.Shared.Enums;

    public class ModuleNavigationModel
    {
        public string? Type { get; set; }
        public double DisplayOrder { get; set; }
        public Guid CourseId { get; set; }
        public Guid? UnitId { get; set; }
        public Guid? ObjectId { get; set; }
        public Guid? CurrentLessonId { get; set; }
        public IList<EnumCourseSkill>? CourseSkills { get; set; }
    }
}
