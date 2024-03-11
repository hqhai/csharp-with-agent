// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.ShareModels
{
    using System;
    using Fsel.Shared.Enums;

    public class TokenHistoryQueueModel
    {
        public Guid TokenConfigId { get; set; }

        public double InitialToken { get; set; }

        public double VolatileToken { get; set; }

        public double RemainToken { get; set; }

        public Guid UserId { get; set; }

        public Guid ObjectId { get; set; }

        public EnumTokenHistoryType Type { get; set; }
    }
}
