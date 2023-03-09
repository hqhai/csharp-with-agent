using Fsel.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fsel.Course.Domain.Entities
{
    public class ExcerciseQuestion : Entity
    {
        public Guid ExcerciseId { get; set; }

        public Guid QuestionId { get; set; }
    }
}