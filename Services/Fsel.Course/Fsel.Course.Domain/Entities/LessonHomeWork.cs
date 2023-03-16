using System.ComponentModel.DataAnnotations;
using Fsel.Core.Entities;
using Fsel.Course.Domain.Enums.ErrorCodes;

namespace Fsel.Course.Domain.Entities
{
    public class LessonHomeWork : Entity
    {
        public Lesson? Lesson { get; set; }
        public HomeWork? HomeWork { get; set; }

        [Required(ErrorMessage = nameof(EnumLessonErrorCode.LS01V))]
        public Guid LessonId { get; set; }

        [Required(ErrorMessage = nameof(EnumHomeWorkErrorCode.HW01V))]
        public Guid HomeWorkId { get; set; }
    }
}
