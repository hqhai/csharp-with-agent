using Fsel.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fsel.Course.Domain.Entities
{
    public class TimeCodeExcercise : Entity
    {
        public Guid ExcerciseId { get; set; }

        public Guid VideoTimeCodeId { get; set; }
    }
}