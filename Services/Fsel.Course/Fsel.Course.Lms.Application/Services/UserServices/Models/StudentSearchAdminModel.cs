// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.UserServices.Models
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class StudentSearchAdminModel : BaseModel
    {
        public string? FullName { get; set; }
        public DateTime? Birthday { get; set; }
        public string? Email { get; set; }
        public EnumCourseType Type { get; set; }
        public Guid? CourseId { get; set; }
        public EnumCourseLevel? CourseLevel { get; set; }
    }
}
