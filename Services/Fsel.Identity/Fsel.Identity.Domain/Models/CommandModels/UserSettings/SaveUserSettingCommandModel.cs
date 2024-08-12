// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.UserSettings
{
    public class SaveUserSettingCommandModel
    {
        public bool NotifiGame { get; set; }

        public bool NotifiFeedBack { get; set; }

        public bool NotifiLesson { get; set; }

        public bool NotifiForum { get; set; }

        public bool NotifiEmail { get; set; }

        public bool IsSoundEffect { get; set; }

        public string? Language { get; set; }
    }
}
