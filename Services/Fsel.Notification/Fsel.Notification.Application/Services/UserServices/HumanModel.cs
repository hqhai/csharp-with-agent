// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Notification.Application.Services.UserServices
{
    using Fsel.Core.Base.BaseModels;

    public class HumanModel : BaseModel
    {
        public string? FullName { get; set; }
        public string? Birthday { get; set; }
        public string? AvatarPath { get; set; }
        public string? UserId { get; set; }
    }
}
