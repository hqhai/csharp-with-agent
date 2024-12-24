// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Enums
{
    using System.ComponentModel;

    public enum EnumOverallScore
    {
        [Description("Hoàn thành đúng < 75%")]
        AccuracyBelow75 = 74,

        [Description("Hoàn thành đúng >= 75%")]
        Accuracy75OrMore,
    }
}
