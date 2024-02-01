// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Entities.Configs
{
    public class TokenCoinConfigs
    {
        public long BaseValue { get; set; }
        public long TotalActions { get; set; }
    }

    public class TokenConfigs
    {
        public long BaseValue { get; set; }
        public long TotalActions { get; set; }
        public string? Description { get; set; }

        public long TotalCoinValue
        {
            get
            {
                return BaseValue * TotalActions;
            }
        }
    }

    public class TokenFocusModeConfigs
    {
        public long BaseValue { get; set; }
        public long TotalActions { get; set; }
        public Guid FocusTimeId { get; set; }
        public int DisplayOrder { get; set; }
        public string? Description { get; set; }

        public long TotalCoinValue
        {
            get
            {
                return BaseValue * TotalActions;
            }
        }
    }

    public class TokenConfigDailyCheckIns
    {
        public long BaseValue { get; set; }
        public long TotalActions { get; set; }
        public int Level { get; set; }
        public string? Description { get; set; }

        public long TotalCoinValue
        {
            get
            {
                return BaseValue * TotalActions;
            }
        }
    }
}
