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
        public Excercise? Excercise { get; set; }

        public Question? Question { get; set; }
        public Guid ExcerciseId { get; set; }

        public Guid QuestionId { get; set; }
    }
}