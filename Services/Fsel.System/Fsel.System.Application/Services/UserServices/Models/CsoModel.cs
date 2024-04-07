// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Services.UserServices.Models
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class CSOModel : BaseModel
    {
        public IList<EnumCourseType>? CourseTypes { get; set; }
        public IList<EnumCourseLevel>? CourseLevels { get; set; }
        public IList<Guid>? PackageIds { get; set; }
        public string? PassportPath { get; set; }
        public string? UniversityDegreePath { get; set; }
        public string? CertificationPath { get; set; }
        public string? PoliceClearancePath { get; set; }
        public Guid UserId { get; set; }
        public UserModel? User { get; set; }
    }
}
