// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.CommandModels.TokenHistorys
{
    using Fsel.Shared.Enums;
    using global::System;

    public class CreateTokenHistoryCommandModel
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
