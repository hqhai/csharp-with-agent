using Fsel.Core.Base.BaseModels;
using Fsel.Course.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fsel.Course.Domain.Models.EntiyModels
{
    public class CourseUnitMockTestModel
    {
        public int OrderNumber { get; set; }

        public Guid? UnitId { get; set; }

        public Guid? MockTestId { get; set; }
    }
}
