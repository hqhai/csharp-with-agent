// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class StudentSearchAdminModel : BaseModel
    {
        public string? FullName { get; set; }
        public DateTime? Birthday { get; set; }
        public string? Email { get; set; }
        public EnumCourseType Type { get; set; }
        public EnumCourseLevel? CourseLevel { get; set; }
        public Guid? SchoolId { get; set; }
        public string? SchoolName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Grade { get; set; }
        public string? Class { get; set; }
        public string? UserName { get; set; }
    }
}
