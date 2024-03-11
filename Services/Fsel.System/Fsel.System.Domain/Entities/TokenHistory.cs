// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Entities
{
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;

    public class TokenHistory : Entity
    {
        public Guid TokenConfigId { get; set; }

        public int InitialToken { get; set; }

        public int VolatileToken { get; set; }

        public int RemainToken { get; set; }

        public Guid UserId { get; set; }

        public EnumTokenHistoryType Type { get; set; }

        public TokenConfig? TokenConfig { get; set; }
    }
}
