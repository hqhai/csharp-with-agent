// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.EntityModels
{
    public class SenderModel
    {
        public Guid SenderId { get; set; }
        public string? FullName { get; set; }
        public string? Code { get; set; }
    }
}
