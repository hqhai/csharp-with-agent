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
    public class UnitLesson : Entity
    {
        public Lesson? Lesson { get; set; }
        public Unit? Unit { get; set; }

        [Required(ErrorMessage = nameof(EnumUnitLessonErrorCode.LU01C))]
        public Guid LessonId { get; set; }

        [Required(ErrorMessage = nameof(EnumUnitLessonErrorCode.LU01C))]
        public Guid UnitId { get; set; }
    }
}