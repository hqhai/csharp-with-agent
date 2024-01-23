// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Entities
{
    using System.ComponentModel.DataAnnotations.Schema;
    using Fsel.Common.Helpers;
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;

    public class UrBoxTransaction : Entity
    {
        public EnumUrBoxTransactionStatus Status { get; set; }

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
    }
}
