// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.EntityModels
{
    using System;
    using Fsel.Common.Helpers;
    using Fsel.Shared.Enums;

    public class HumanProfileModel
    {
        public Guid Id { get; set; }
        public string? FullName { get; set; }
        public DateTime? Birthday { get; set; }
        public string? Code { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public EnumGender? Gender { get; set; }
        public string? Email { get; set; }

        private string? _avatarPath;

        public string? AvatarPath
        {
            set { _avatarPath = value; }
            get { return _avatarPath.AddS3BaseUrl(); }
        }

        public string? Role { get; set; }
        public Guid? UserId { get; set; }

        public UserInformationModel? User { get; set; }
    }
}
