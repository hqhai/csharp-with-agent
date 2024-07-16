// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.ShareModels
{
    using Fsel.Shared.Enums;

    public class OrderQueueModel
    {
        public Guid OrderId { get; set; }
        public Guid UserId { get; set; }
        public EnumOrderStatus Status { get; set; }
    }
}
