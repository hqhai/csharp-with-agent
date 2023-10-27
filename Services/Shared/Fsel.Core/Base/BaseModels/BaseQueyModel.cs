// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.Constants;

namespace Fsel.Core.Base.BaseModels
{
    public class BaseQueyModel
    {
        public string? Keyword { get; set; }

        public int Page { get; set; } = PagingValues.Page;

        public int PageSize { get; set; } = PagingValues.PageSize;
    }
}
