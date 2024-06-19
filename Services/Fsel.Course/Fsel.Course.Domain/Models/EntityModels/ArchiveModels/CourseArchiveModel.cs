// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.ArchiveModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class CourseArchiveModel : BaseModel
    {
        public string? Code { get; set; }
        public EnumCourseLevel CourseLevel { get; set; }
        public DateTime? DeletedDate { get; set; }
        public IList<CourseTeacherModel>? CourseTeachers { get; set; }
    }
}
