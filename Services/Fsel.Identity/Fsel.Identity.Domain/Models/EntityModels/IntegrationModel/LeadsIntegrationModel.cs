// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.EntityModels.IntegrationModel
{
    using Fsel.Shared.Enums;

    public class LeadsIntegrationModel : IntegrationModel
    {
        public EnumIntegrationStatus? Status { get; set; }

        public DateTime? StartTrial { get; set; }

        public DateTime? ExpireDate { get; set; }

        public string? StatusPT { get; set; }

        public long? AccessTime { get; set; }

    }
}
