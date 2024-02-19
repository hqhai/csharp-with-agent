// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Models.QueryModels.UrBox
{
    using Fsel.Core.Base.BaseModels;

    public class SearchGiftQueryModel : BaseQueryModel
    {
        public int? CategoryId { get; set; }
        public long? Min { get; set; }
        public long? Max { get; set; }
        public bool? IsPopular { get; set; }
        public bool? IsLatest { get; set; }
    }
}
