using Fsel.Core.Entities;
using Fsel.Course.Domain.Enums.ErrorCodes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fsel.Course.Domain.Entities
{
    public class LessonExtraPractice : Entity
    {
        public Lesson? Lesson { get; set; }
        public ExtraPractice? ExtraPractice { get; set; }
        public Guid LessonId { get; set; }
        public Guid ExtracPraticeId { get; set; }
    }
}