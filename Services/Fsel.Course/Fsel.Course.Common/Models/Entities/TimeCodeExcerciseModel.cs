using Fsel.Core.Base.BaseModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fsel.Course.Common.Models.Entities
{
    public class TimeCodeExcerciseModel
    {
        public Guid ExcerciseId { get; set; }

        public List<ExcerciseModel>? Exercices { get; set; }

        public Guid VideoTimeCodeId { get; set; }
    }
}