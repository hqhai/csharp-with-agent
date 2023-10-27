// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.UserServices.Models
{
    public class HumanModel
    {
        public string? FullName { get; set; }
        public DateTime? Birthday { get; set; }
        public string? AvatarPath { get; set; }
        public string? Code { get; set; }
        public Guid? UserId { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
    }
}
