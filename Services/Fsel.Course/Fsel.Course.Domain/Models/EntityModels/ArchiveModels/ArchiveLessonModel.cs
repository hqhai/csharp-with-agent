// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.ArchiveModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Enums;
    using Fsel.Shared.Enums;

    public class ArchiveLessonModel : BaseModel
    {
        public string? Name { get; set; }
        public EnumCourseLevel CourseLevel { get; set; }
        public Guid TeacherId { get; set; }
        public string? TeacherName { get; set; }
        public EnumTimeCodeType? TimeCodeType { get; set; }
        public DateTime? DeletedDate { get; set;}
    }
}
