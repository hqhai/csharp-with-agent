// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.ShareModels
{
    using System.Collections.Generic;
    using Fsel.Shared.Enums;

    public class SendSMSCommandModel
    {
        public IList<string>? PhoneNumbers { get; set; }
        public string? Content { get; set; }
        public EnumSendSMSTemplate? Template { get; set; }
        public object? Params { get; set; }
        public bool IsCheckDuplicate { get; set; } = true;
        public EnumSendSMSPriority Priority { get; set; } = EnumSendSMSPriority.High;
    }
}
