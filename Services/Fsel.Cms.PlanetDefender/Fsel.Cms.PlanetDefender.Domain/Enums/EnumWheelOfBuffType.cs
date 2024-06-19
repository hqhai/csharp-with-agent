// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Domain.Enums
{
    using System.ComponentModel;

    public enum EnumWheelOfBuffType
    {
        [Description("Receive Z Buff (random)")]
        ReceiveZBuff,

        [Description("10 FSEL coin")]
        TenFSELcoin,

        [Description("Heal Bar/Shield (+10%)")]
        HealBarShield,

        [Description("20 FSEL coin")]
        TwentyFSELcoin,

        [Description("50 FSEL coin")]
        FiftyFSELcoin,

        [Description("X")]
        X

    }
}
