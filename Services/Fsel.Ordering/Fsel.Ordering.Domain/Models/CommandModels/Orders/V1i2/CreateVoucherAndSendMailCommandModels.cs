// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Models.CommandModels.Orders.V1i2
{
    public class CreateVoucherAndSendMailCommandModels
    {
        public IList<CreateVoucherAndSendMailCommandModel>? UserVoucher { get; set; }
    }

    public class CreateVoucherAndSendMailCommandModel
    {
        public string? Email { get; set; }
        public int MonthNumber { get; set; }
    }
}
