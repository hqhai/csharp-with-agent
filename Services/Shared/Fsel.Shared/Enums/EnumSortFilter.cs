// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Enums
{
    using System.ComponentModel;

    public enum EnumSortFilter
    {
        [Description("Newest")]
        Newest,

        [Description("Oldest")]
        Oldest,

        [Description("Most popular")]
        MostPopular,

        [Description("Trending now")]
        TrendingNow
    }
}
