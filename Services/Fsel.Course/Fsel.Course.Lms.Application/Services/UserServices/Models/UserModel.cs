// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.UserServices.Models
{
    using Fsel.Core.Base.BaseModels;

    public class UserModel : BaseModel
    {
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? School { get; set; }
        public string? SchoolName { get; set; }
        public HumanModel? Human { get; set; }
    }
}
