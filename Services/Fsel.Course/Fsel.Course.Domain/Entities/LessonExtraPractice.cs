using System.ComponentModel.DataAnnotations;
using Fsel.Core.Entities;
using Fsel.Course.Domain.Enums.ErrorCodes;

namespace Fsel.Course.Domain.Entities
{
    public class LessonExtraPractice : Entity
    {
        public Lesson? Lesson { get; set; }
        public ExtraPractice? ExtraPractice { get; set; }

        [Required(ErrorMessage = nameof(EnumLessonErrorCode.LS01V))]
        public Guid LessonId { get; set; }

        [Required(ErrorMessage = nameof(EnumExtraPractiveErrorCode.EP01V))]
        public Guid ExtracPraticeId { get; set; }
    }
}
