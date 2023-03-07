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
    public class LessonVideo : Entity
    {
        public Lesson? Lesson { get; set; }
        public Video? Video { get; set; }

        [Required(ErrorMessage = nameof(EnumLessonVideoErrorCode.LV01C))]
        public Guid LessonId { get; set; }

        [Required(ErrorMessage = nameof(EnumLessonVideoErrorCode.LV01C))]
        public Guid VideoId { get; set; }
    }
}
