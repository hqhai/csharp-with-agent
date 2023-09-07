// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Notification.Application.Services.UserServices
{
    using Fsel.Core.Base.BaseModels;

    public class UserModel : BaseModel
    {
        public string? FullName { get; set; }
        public HumanModel? Human { get; set; }
    }
}
