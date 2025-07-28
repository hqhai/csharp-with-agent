using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fsel.Course.Lms.Application.Services.SystemService.QueryModels
{
    public class GetUnitChatbotConfigsQueryModel
    {
        public IList<Guid>? UnitIds { get; set; }
    }
}
