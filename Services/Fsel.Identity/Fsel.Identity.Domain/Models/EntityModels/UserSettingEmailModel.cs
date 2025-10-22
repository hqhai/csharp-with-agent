// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.EntityModels
{
    public class UserSettingEmailModel
    {
        public string? Email { get; set; }

        public IList<UserSenderSettingModel>? UserSenderSettings { get; set; }
    }
}
