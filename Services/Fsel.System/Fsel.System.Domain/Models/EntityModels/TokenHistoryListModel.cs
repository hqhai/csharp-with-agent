// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.EntityModels
{
    using Fsel.Shared.Enums;
    using global::System;
    using global::System.Collections.Generic;

    public class TokenHistoryListModel
    {
        public DateTime Date { get; set; }

        public IList<FeatureModel>? Features { get; set; }
    }

    public class FeatureModel
    {
        public double InitialToken { get; set; }

        public double VolatileToken { get; set; }

        public double RemainToken { get; set; }
        public EnumTokenFeature Feature { get; set; }

        public EnumTokenHistoryType Type { get; set; }

        public IList<TokenHistoryModel>? TokenHistories { get; set; }
    }
}
