// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Domain.Models.EntityModels
{
    public class RankingGameModel
    {
        public string? NickName { get; set; }
        public string? TagName { get; set; }
        public long TotalScore { get; set; }
        public int Level { get; set; }
        public bool IsStudent { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
