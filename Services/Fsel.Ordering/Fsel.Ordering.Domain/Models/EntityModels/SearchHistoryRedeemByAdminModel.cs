// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Models.EntityModels
{
    using System;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class SearchHistoryRedeemByAdminModel : BaseModel
    {
        public Guid ProductId { get; set; }
        public string? Code { get; set; }
        public string? ProductCode { get; set; }
        public string? StudentCode { get; set; }
        public string? StudentName { get; set; }
        public string? Email { get; set; }
        public string? School { get; set; }
        public EnumOrderTransactionStatus Status { get; set; }
    }
}
