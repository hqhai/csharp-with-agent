// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Models.CommandModels.Vouchers
{
    using Fsel.Core.Base.BaseModels;

    public class UpdateVoucherStatusCommandModel : BaseCommandModel
    {
        public bool? IsActive { get; set; }
    }
}
