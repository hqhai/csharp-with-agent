// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Sender.Application.Services.UserServices.Models
{
    public class UserSettingEmailModel
    {
        public string? Email { get; set; }

        public IList<UserSenderSettingModel>? UserSenderSettings { get; set; }
    }
}
