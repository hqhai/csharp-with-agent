// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Entities
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;

    public class OrderTransaction : Entity
    {
        public EnumOrderTransactionStatus Status { get; set; }
        public EnumOrderTransactionType Type { get; set; }
        public Guid? OrderId { get; set; }
        public Guid? ProductId { get; set; }

        [MaxLength(100, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Code { get; set; }

        public string? RequestBodyStr { get; set; }

        [NotMapped]
        public object? RequestBody
        {
            get { return RequestBodyStr.Deserialize<object>(); }
            set { RequestBodyStr = value.Serialize(); }
        }

        public string? ResponseBodyStr { get; set; }

        [NotMapped]
        public object? ResponseBody
        {
            get { return ResponseBodyStr.Deserialize<object>(); }
            set { ResponseBodyStr = value.Serialize(); }
        }

        public Order? Order { get; set; }
        public Product? Product { get; set; }
    }
}
