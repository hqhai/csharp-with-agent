using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fsel.Core.Base.BaseModels
{
    public class PagingInfoModel
    {
        public int PageSize { get; set; }

        public int Page { get; set; }

        public long TotalItems { get; set; }
    }
}
