// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Services.SystemServices.Models
{
    using System;

    public class GetUnitChatbotConfigsQueryModel
    {
        public IList<Guid>? UnitIds { get; set; }
    }
}
