// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.ArchiveModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class ArchiveUnitModel : BaseModel
    {
        public string? Code { get; set; }
        public EnumCourseLevel CourseLevel { get; set; }
        public IList<TeacherModel>? Teachers { get; set; }
    }
    public class TeacherModel
    {
        public Guid TeacherId { get; set; }
        public string? TeacherName { get; set; }
    }
}
