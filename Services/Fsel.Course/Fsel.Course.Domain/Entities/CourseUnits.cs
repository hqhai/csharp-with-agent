using Fsel.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fsel.Course.Domain.Entities
{
    public class CourseUnits : Entity
    {
        public Unit? Unit { get; set; }

        public Course? Course { get; set; }

        public Guid UnitId { get; set; }

        public Guid CourseId { get; set; }
    }
}