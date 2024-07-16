// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.ShareModels
{
    public class OrderQueueModel
    {
        public Guid OrderId { get; set; }
        public Guid UserId { get; set; }
        public string? Status { get; set; }
    }
}
