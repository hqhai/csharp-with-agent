// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.UserSettings
{
    using Fsel.Core.Base.BaseModels;

    public class SaveUserSettingCommandModel : BaseCommandModel
    {
        public bool NotifiGame { get; set; }

        public bool NotifiFeedBack { get; set; }

        public bool NotifiLesson { get; set; }

        public bool NotifiForum { get; set; }

        public string? Language { get; set; }
    }
}
