using System.ComponentModel.DataAnnotations;
using Fsel.Core.Entities;
using Fsel.Course.Domain.Enums.ErrorCodes;

namespace Fsel.Course.Domain.Entities
{
    public class LessonVideo : Entity
    {
        public Lesson? Lesson { get; set; }
        public Video? Video { get; set; }

        [Required(ErrorMessage = nameof(EnumLessonErrorCode.LS01V))]
        public Guid LessonId { get; set; }

        [Required(ErrorMessage = nameof(EnumVideoErrorCode.VD01V))]
        public Guid VideoId { get; set; }
    }
}
