// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Services.UserServices.Models
{
    using Fsel.Core.Base.BaseModels;

    public class TeacherModel : BaseModel
    {
        public string? FullName { get; set; }
        public string? PassportPath { get; set; }
        public string? UniversityDegreePath { get; set; }
        public string? CertificationPath { get; set; }
        public string? PoliceClearancePath { get; set; }
        public Guid UserId { get; set; }
        public UserModel? User { get; set; }
    }
}
