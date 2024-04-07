// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.UserServices.Models
{
    using Fsel.Core.Base.BaseModels;

    public class UserModel : BaseModel
    {
        public string? Code { get; set; }
        public string? AvatarPath { get; set; }
        public string? Email { get; set; }
        public DateTime? Birthday { get; set; }
        public string? FullName { get; set; }
    }
}
