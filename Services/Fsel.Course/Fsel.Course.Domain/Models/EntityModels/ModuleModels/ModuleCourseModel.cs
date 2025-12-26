// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.ModuleModels
{
    using Fsel.Course.Domain.Enums;

    public class ModuleCourseModel : BaseModuleModel
    {
        public EnumCourseConfigType CourseConfigType { get; set; }
        public Guid CourseId { get; set; }
        public int ProgressPrecent { get; set; }
    }
}
