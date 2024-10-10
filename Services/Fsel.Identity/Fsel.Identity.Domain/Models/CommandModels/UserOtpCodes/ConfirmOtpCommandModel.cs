// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.UserOtpCodes
{
    using System.Text.Json.Serialization;

    public class ConfirmOtpCommandModel
    {
        public string? Otp { get; set; }
        public string? Email { get; set; }

        [JsonIgnore]
        public bool IsCheckExpiredTime { get; set; } = true;
    }
}
