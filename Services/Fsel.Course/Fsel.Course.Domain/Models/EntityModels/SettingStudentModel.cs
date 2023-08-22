// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Shared.Enums;

    public class SettingStudentModel
    {
        public bool IsPlacementTest { get; set; }
        public Guid? ClassId { get; set; }
        public EnumCourseLevel Level { get; set; }
        public EnumCourseLevel? LevelNext { get; set; }
    }
}
