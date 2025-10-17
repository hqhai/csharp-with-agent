// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.QueryModels.Integration
{
    using Fsel.Core.Base.BaseModels;

    public class IntegrationQueryModel : BaseQueryModel
    {
        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string? Email { get; set; }

        public string? UserName { get; set; }
    }
}
