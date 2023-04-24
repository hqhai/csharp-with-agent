// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.Users
{
    using Fsel.Shared.Enums;

    public class CreateUserCommandModel
    {
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public DateTime? Birthday { get; set; }
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public IList<EnumCourseType>? Types { get; set; }
        public IList<EnumCSORole>? CSORoles { get; set; }
        public IList<EnumTeacherRole>? TeacherRoles { get; set; }
        public IList<EnumCourseLevel>? CourseLevels { get; set; }
        public EnumRoleRegisterWithAdmin Role { get; set; }
    }
}
