using Fsel.Core.Entities;

namespace Fsel.Course.Domain.Entities
{
    public class TimeCodeExcercise : Entity
    {
        public VideoTimeCode? VideoTimeCode { get; set; }
        public Excercise? Excercise { get; set; }
        public Guid ExcerciseId { get; set; }

        public Guid VideoTimeCodeId { get; set; }
    }
}