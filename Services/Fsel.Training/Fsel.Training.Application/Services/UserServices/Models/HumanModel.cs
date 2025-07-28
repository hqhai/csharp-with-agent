// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Services.UserServices.Models
{
    using Fsel.Core.Base.BaseModels;

    public class HumanModel : BaseModel
    {
        public string? FullName { get; set; }
        public DateTime? Birthday { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Code { get; set; }
        public string? AvatarPath { get; set; }
    }
}
