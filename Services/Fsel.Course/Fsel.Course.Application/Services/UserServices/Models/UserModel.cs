// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Services.UserServices.Models
{
    using Fsel.Core.Base.BaseModels;

    public class UserModel : BaseModel
    {
        public string? Code { get; set; }
        public string? FullName { get; set; }
        public string? AvatarPath { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
    }
}
