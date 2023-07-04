// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Entities
{
    using Fsel.Core.Entities;

    public class VoucherPackage : Entity
    {
        public double Percentage { get; set; }

        public Guid PacketId { get; set; }

        public Guid VoucherId { get; set; }

        public Package? Packet { get; set; }

        public Voucher? Voucher { get; set; }
    }
}
