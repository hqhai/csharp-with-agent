// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.ShareModels
{
    using System;

    public class TokenCoinConfigs
    {
        public long BaseValue { get; set; }
        public long TotalActions { get; set; }
    }

    public class TokenConfigs : TokenCoinConfigs
    {
        public string? Description { get; set; }

        public long TotalCoinValue
        {
            get
            {
                return BaseValue * TotalActions;
            }
        }
    }

    public class TokenConfigFocusModes : TokenConfigs
    {
        public Guid FocusTimeId { get; set; }
        public double TargetTime { get; set; }
        public int DisplayOrder { get; set; }
    }

    public class TokenConfigDailyCheckIns : TokenConfigs
    {
        public int Level { get; set; }
    }
}
