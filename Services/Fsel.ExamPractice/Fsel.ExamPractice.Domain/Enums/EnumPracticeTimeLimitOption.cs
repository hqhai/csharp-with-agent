// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Domain.Enums
{
    using System.ComponentModel;

    public enum EnumPracticeTimeLimitOption
    {
        [Description("Không giới hạn")]
        Unlimited = 0,

        [Description("Theo đề thi")]
        ExamBased = 1,

        [Description("30 phút")]
        Minute30 = 1800,

        [Description("45 phút")]
        Minute45 = 2700,

        [Description("60 phút")]
        Minute60 = 3600,

        [Description("75 phút")]
        Minute75 = 4500,

        [Description("90 phút")]
        Minute90 = 5400,
    }
}
