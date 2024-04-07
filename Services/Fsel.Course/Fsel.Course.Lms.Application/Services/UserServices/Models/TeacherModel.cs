// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.UserServices.Models
{
    using Fsel.Core.Base.BaseModels;

    public class TeacherModel : BaseModel
    {
        public string? PassportPath { get; set; }
        public string? UniversityDegreePath { get; set; }
        public string? CertificationPath { get; set; }
        public string? PoliceClearancePath { get; set; }
        public Guid UserId { get; set; }
        public UserModel? User { get; set; }
    }
}
