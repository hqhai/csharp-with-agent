using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fsel.Core.Base.BaseModels
{
    public class PagingItemsModel<T>
    {
        public IEnumerable<T>? Items { get; set; }

        public PagingInfoModel? PagingInfo { get; set; }
    }
}
