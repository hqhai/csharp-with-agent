// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Entities
{
    using System.ComponentModel.DataAnnotations;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Entities;

    public class UserSetting : Entity
    {
        public UserSetting()
        { }

        public UserSetting(bool setDefaultValue)
        {
            if (setDefaultValue)
            {
                NotifiGame = true;
                NotifiFeedBack = true;
                NotifiLesson = true;
                NotifiForum = true;
                NotifiEmail = true;
                IsSoundEffect = true;
                Language = "vn";
            }
        }

        public Guid? UserId { get; set; }

        public bool NotifiGame { get; set; }

        public bool NotifiFeedBack { get; set; }

        public bool NotifiLesson { get; set; }

        public bool NotifiForum { get; set; }

        public bool NotifiEmail { get; set; }

        public bool IsSoundEffect { get; set; }

        /// <summary>
        /// Tên ngôn ngữ
        /// </summary>
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Language { get; set; }

        public User? User { get; set; }

        public ICollection<UserSenderSetting> UserSenderSettings { get; set; } = new List<UserSenderSetting>();
    }
}
