// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Domain.Models.QueryModels.NewsAndUpdates
{
    using Fsel.Cms.PlanetDefender.Domain.Enums;
    using Fsel.Core.Base.BaseModels;

    public class SearchEventQueryModel : BaseQueryModel
    {
        public bool? Status { get; set; }

        public EnumEventType? Type { get; set; }
    }
}
