using System.ComponentModel.DataAnnotations;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Core.Entities;

namespace Fsel.Course.Domain.Entities
{
    public class UnitLesson : Entity
    {
        public Lesson? Lesson { get; set; }
        public Unit? Unit { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public Guid LessonId { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public Guid UnitId { get; set; }
        public ICollection<UnitLessonResult> UnitLessonResults { get; set; } = new List<UnitLessonResult>();
    }
}
