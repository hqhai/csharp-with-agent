using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Enums.ErrorCodes;
using Fsel.Course.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fsel.Core.Base.BaseModels;

namespace Fsel.Course.Domain.Models.EntiyModels
{
    public class VideoTimeCodeModel : BaseEntityModel
    {
        public long DisplayTimeTicks { get; set; }

        public long ExecutionTimeTicks { get; set; }

        public EnumTimeCodeType TimeCodeType { get; set; }

        public Guid VideoId { get; set; }

        public TimeSpan DisplayTime { get; set; }

        public TimeSpan ExecutionTime { get; set; }

        public List<ExcerciseModel>? Excercises { get; set; }
    }
}
