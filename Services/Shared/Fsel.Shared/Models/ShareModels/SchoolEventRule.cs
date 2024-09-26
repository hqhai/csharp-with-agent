// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.ShareModels
{
    public class SchoolEventRule
    {
        public bool LuckySpin { get; set; }
        public int? PaymentMonth { get; set; }
        public DateTime? PaymentDate { get; set; }
        public IList<EnumSchoolEventRuleAction>? Actions { get; set; }
        public EnumByPassPaymentType ByPassPaymentType { get; set; }
        public bool IsByPassPayment { get; set; }
        public DateTime? RegisterStartDate { get; set; }
        public DateTime? RegisterEndDate { get; set; }
        public DateTime? StartDate => WeekEvents?.FirstOrDefault()?.StartDate;
        public DateTime? EndDate => WeekEvents?.LastOrDefault()?.EndDate;
        public DateTime? AwardStartDate { get; set; }
        public DateTime? AwardEndDate { get; set; }
        public string? LinkLeaderBoard { get; set; }
        public string? LinkLuckyStar { get; set; }
        public string? LeaderBoardGiftImage { get; set; }
        public string? LuckySpinGiftImage { get; set; }
        public IList<WeekEvent>? WeekEvents { get; set; }
        public IList<WeekEvent>? LuckyStarRules { get; set; }
    }

    public enum EnumSchoolEventRuleAction
    {
        StopAtLevelSelection,
        DisableLeaderBoard
    }

    public enum EnumByPassPaymentType
    {
        Month,
        Date
    }

    public class WeekEvent
    {
        public string? Title { get; set; }
        public IList<WeekRule>? Rules { get; set; }
        public int PrizeCount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int WeekNumber { get; set; }
        public string? Rule { get; set; }
    }

    public class WeekRule
    {
        public string? PrizeQuantity { get; set; }
        public string? Detail { get; set; }
        public string? RewardImagine { get; set; }
        public int Rank { get; set; }
    }
}
