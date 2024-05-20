// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.Auths
{
    using Fsel.Identity.Domain.Models.CommandModels.Students;

    public class UserLockCommandModel : CreateOrdersFromCRMCommandModel
    {
        public bool IsLock { get; set; }
    }
}
