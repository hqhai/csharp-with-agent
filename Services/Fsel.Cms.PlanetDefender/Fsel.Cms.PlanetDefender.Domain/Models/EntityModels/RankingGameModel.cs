// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;

    public class RankingGameModel : BaseModel
    {
        public string? NickName { get; set; }
        public string? TagName { get; set; }
        public long TotalScore { get; set; }
        public int Level { get; set; }
        public bool IsStudent { get; set; }
    }
}
