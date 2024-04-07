// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Services.UserServices.Models
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class TeacherModel : BaseModel
    {
        public IList<EnumCourseLevel>? CourseLevels { get; set; }
        public IList<EnumCourseType>? LiveCourseTypes { get; set; }
        public IList<EnumCourseType>? CourseTypes { get; set; }
        public string? PassportPath { get; set; }
        public string? UniversityDegreePath { get; set; }
        public string? CertificationPath { get; set; }
        public string? PoliceClearancePath { get; set; }
        public Guid UserId { get; set; }
        public UserModel? User { get; set; }
    }
}
