using Fsel.Core.Entities;

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