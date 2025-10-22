// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Services.UserServices.Models
{
    public class HumanProfileModel
    {
        public string? FullName { get; set; }
        public string? Birthday { get; set; }
        public string? AvatarPath { get; set; }
        public string? Email { get; set; }
        public Guid? UserId { get; set; }
        public Guid? CourseId { get; set; }
        public string? Code { get; set; }
        public UserModel? User { get; set; }
    }
}
