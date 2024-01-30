// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Domain.Models.QueryModels.GameHistorys
{
    using Fsel.Cms.PlanetDefender.Domain.Enums;
    using Fsel.Core.Base.BaseModels;

    public class SearchRankingGameQueryModel : BaseQueryModel
    {
        public EnumRanking Ranking { get; set; }
    }
}
