// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Models.CommandModels.Vouchers
{
    public class CreateVoucherForMasterAgencyCommandModel
    {
        public string? MasterAgency { get; set; }
        public int Package { get; set; }
        public int Quantity { get; set; }
    }
}
