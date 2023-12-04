// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Shared.Enums;

    public class StudentSettingModel
    {
        public bool IsPlacementTest { get; set; }
        public Guid? ClassId { get; set; }
        public EnumCourseLevel Level { get; set; }
        public EnumPlacementTestLevel? PTLevel { get; set; }
        public EnumCourseLevel? StartPTLevel { get; set; }
        public int ModuleNumber { get; set; }
        public bool IsLockPT { get; set; }
        public EnumOrderStatus? Status { get; set; }
        public string? BeginnerGuides { get; set; }
    }
}
