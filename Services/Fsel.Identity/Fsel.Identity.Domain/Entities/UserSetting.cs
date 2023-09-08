// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Entities
{
    using System.ComponentModel.DataAnnotations;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Entities;

    public class UserSetting : Entity
    {
        public string? UserId { get; set; }

        public bool NotifiGame { get; set; }

        public bool NotifiFeedBack { get; set; }

        public bool NotifiLesson { get; set; }

        public bool NotifiForum { get; set; }

        /// <summary>
        /// Tên ngôn ngữ
        /// </summary>
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Language { get; set; }

        public User? User { get; set; }
    }
}
