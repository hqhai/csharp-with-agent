// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.EntityModels
{
    using Fsel.Shared.Enums;

    public class UserSearchModel
    {
        public Guid? Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public int NumberClass { get; set; }
        public string? LiveCourseTypesStr { get; set; }
        public string? CourseTypesStr { get; set; }
        public IList<EnumRoleTeacher>? RoleTeachers { get; set; }
        public EnumRoleRegisterWithAdmin Role { get; set; }
        public bool Status { get; set; }
        public string? FullName { get; set; }
        public Guid? TeacherId { get; set; }
        public Guid? CSOId { get; set; }
        public EnumUserStatus? UserStatus { get; set; }
    }
}
