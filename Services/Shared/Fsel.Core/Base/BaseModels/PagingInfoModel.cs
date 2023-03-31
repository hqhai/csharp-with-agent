// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Core.Base.BaseModels
{
    public class PagingInfoModel
    {
        public int PageSize { get; set; }

        public int Page { get; set; }

        public long TotalItems { get; set; }
    }
}
