// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Services.SystemServices.Models
{
    using System;
    using Fsel.Shared.Enums;

    public class UnitChatbotStatusModel
    {
        public Guid UnitId { get; set; }
        public EnumChatbotConfigStatus Status { get; set; }
    }
}
