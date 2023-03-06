using Fsel.Common.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fsel.Core.Base.BaseModels
{
    public class BaseQueyModel
    {
        public string? Keyword { get; set; }

        public int Page { get; set; } = PagingValues.Page;

        public int PageSize { get; set; } = PagingValues.PageSize;
    }
}
