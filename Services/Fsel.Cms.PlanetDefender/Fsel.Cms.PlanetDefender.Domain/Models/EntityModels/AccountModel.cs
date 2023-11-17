// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class AccountModel : BaseModel
    {
        public string? Name { get; set; }
        public string? AvatarPath { get; set; }
        public string? Code { get; set; }
        public string? UserName { get; set; }
        public string? AccountType { get; set; }
        public EnumGameCourseLevel GameLevel { get; set; }
        public int CurrentRank { get; set; }
        public int Achievement { get; set; }
        public int Level { get; set; }
        public int TotalGamePlay { get; set; }
        public int CharacterOwned { get; set; }
        public int SpaceshipOwned { get; set; }
        public int Exp { get; set; }
        public int TotalExp { get; set; }
        public int FSELCoin { get; set; }
        public IList<GameHistoryModel>? GameHistory { get; set; }
    }
}
