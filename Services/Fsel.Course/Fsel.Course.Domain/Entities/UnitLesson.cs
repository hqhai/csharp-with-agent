using System.ComponentModel.DataAnnotations;
using Fsel.Core.Entities;
using Fsel.Course.Domain.Enums.ErrorCodes;

namespace Fsel.Course.Domain.Entities
{
    public class UnitLesson : Entity
    {
        public Lesson? Lesson { get; set; }
        public Unit? Unit { get; set; }

        [Required(ErrorMessage = nameof(EnumLessonErrorCode.LS01V))]
        public Guid LessonId { get; set; }

        [Required(ErrorMessage = nameof(EnumUnitErrorCode.U01V))]
        public Guid UnitId { get; set; }
    }
}
