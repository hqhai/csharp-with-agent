// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.UserServices.CommandModels
{
    using Fsel.Shared.Enums;

    public class UpdateSenderSettingCommandModel
    {
        public Guid UserId { get; set; }

        public EnumSenderTemplate Template { get; set; }
    }
}
