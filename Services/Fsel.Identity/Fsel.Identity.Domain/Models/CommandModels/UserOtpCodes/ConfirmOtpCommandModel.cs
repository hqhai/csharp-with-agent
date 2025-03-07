// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.UserOtpCodes
{
    using System.ComponentModel.DataAnnotations;
    using System.Text.Json.Serialization;

    public class ConfirmOtpCommandModel
    {
        [Required]
        public string? Otp { get; set; }

        public string? Email { get; set; }

        public string? PhoneNumber { get; set; }

        [JsonIgnore]
        public bool IsCheckExpiredTime { get; set; } = true;

        public Guid? UserId { get; set; }
    }
}
