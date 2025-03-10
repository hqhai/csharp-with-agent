// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Models.QueryModels.Products
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;

    public class SearchProductQueryModel : BaseQueryModel
    {
        #region Dùng cho LMS Admin

        public string? Code { get; set; }
        public string? Name { get; set; }
        public EnumProductStatus? Status { get; set; }

        #endregion Dùng cho LMS Admin

        #region Dùng cho LMS

        public IList<Guid>? EventIds { get; set; }

        private string? _eventIdsStr;

        public string? EventIdsStr
        {
            get { return _eventIdsStr; }
            set
            {
                _eventIdsStr = value;
                if (!string.IsNullOrEmpty(value))
                {
                    EventIds = value.ToList<Guid>();
                }
            }
        }

        public bool? PopularOrLatest { get; set; }
        public int? Min { get; set; }
        public int? Max { get; set; }

        #endregion Dùng cho LMS
    }
}
