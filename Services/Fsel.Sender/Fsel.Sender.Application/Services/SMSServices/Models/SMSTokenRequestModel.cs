// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Sender.Application.Services.SMSServices.Models
{
    using Refit;

    public class SMSTokenRequestModel
    {
        [AliasAs("grant_type")]
        public string? GrantType { get; set; }
    }
}
