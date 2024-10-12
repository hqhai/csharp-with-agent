// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Entities
{
    using Fsel.Core.Entities;

    public class UserConfig : Entity
    {
        public Guid UserId { get; set; }
        public bool IsViewNewFeature { get; set; }
    }
}
