using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fsel.Course.Lms.Application.Services.UserServices.Models
{
    public class GetStudentIdsInEventByStudentIdsQueryModel
    {
        public IList<Guid>? StudentIds { get; set; }
    }
}
