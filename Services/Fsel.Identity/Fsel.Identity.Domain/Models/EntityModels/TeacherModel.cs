// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.EntityModels
{
    using Fsel.Common.Helpers;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class TeacherModel : BaseModel
    {
        public IList<EnumCourseType>? CourseTypes { get; set; }
        public IList<EnumCourseLevel>? CourseLevels { get; set; }
        public IList<EnumCourseType>? LiveCourseTypes { get; set; }
        public int NumberClass { get; set; }

        private string? _passportPath;

        public string? PassportPath
        {
            set { _passportPath = value; }
            get { return _passportPath.AddS3BaseUrl(); }
        }

        private string? _universityDegreePath;

        public string? UniversityDegreePath
        {
            set { _universityDegreePath = value; }
            get { return _universityDegreePath.AddS3BaseUrl(); }
        }

        private string? _certificationPath;

        public string? CertificationPath
        {
            set { _certificationPath = value; }
            get { return _certificationPath.AddS3BaseUrl(); }
        }

        private string? _policeClearancePath;

        public string? PoliceClearancePath
        {
            set { _policeClearancePath = value; }
            get { return _policeClearancePath.AddS3BaseUrl(); }
        }

        public IList<TeacherBankAccountModel>? TeacherBankAccounts { get; set; }
        public UserModel? User { get; set; }
        public Guid UserId { get; set; }
    }
}
