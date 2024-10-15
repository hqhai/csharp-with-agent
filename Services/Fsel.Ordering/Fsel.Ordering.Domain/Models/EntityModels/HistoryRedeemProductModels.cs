// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class HistoryRedeemProductModels
    {
        public IList<HistoryRedeemProductModel> Requested { get; set; } = new List<HistoryRedeemProductModel>();
        public IList<HistoryRedeemProductModel> Received { get; set; } = new List<HistoryRedeemProductModel>();
    }

    public class HistoryRedeemProductModel : BaseModel
    {
        public EnumOrderTransactionStatus Status { get; set; }
        public EnumOrderTransactionType Type { get; set; }
        public object? RequestBody { get; set; }
        public string? Code { get; set; }
        public ProductModel? Product { get; set; }
    }
}
