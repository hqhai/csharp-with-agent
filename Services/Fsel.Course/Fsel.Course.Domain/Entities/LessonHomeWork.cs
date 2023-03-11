using Fsel.Core.Entities;
using Fsel.Course.Domain.Enums.ErrorCodes;
using System.ComponentModel.DataAnnotations;

namespace Fsel.Course.Domain.Entities
{
    public class LessonHomeWork : Entity
    {
        public Lesson? Lesson { get; set; }
        public HomeWork? HomeWork { get; set; }

        [Required(ErrorMessage = nameof(EnumLessonErrorCode.LS01C))]
        public Guid LessonId { get; set; }

        [Required(ErrorMessage = nameof(EnumHomeWorkErrorCode.HW01C))]
        public Guid HomeWorkId { get; set; }
    }
}