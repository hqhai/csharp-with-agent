using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fsel.Course.Domain.Models.EntiyModels
{
    public class TimeCodeExcerciseModel
    {
        public Guid ExcerciseId { get; set; }

        public List<ExcerciseModel>? Exercices { get; set; }

        public Guid VideoTimeCodeId { get; set; }
    }
}
