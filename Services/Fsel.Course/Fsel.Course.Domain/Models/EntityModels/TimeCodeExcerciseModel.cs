namespace Fsel.Course.Domain.Models.EntityModels
{
    public class TimeCodeExcerciseModel
    {
        public Guid ExcerciseId { get; set; }

        public ICollection<ExcerciseModel>? Exercices { get; set; }

        public Guid VideoTimeCodeId { get; set; }
    }
}
