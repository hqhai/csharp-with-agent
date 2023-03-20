namespace Fsel.Course.Domain.Models.EntityModels
{
    public class TimeCodeExerciseModel
    {
        public Guid ExerciseId { get; set; }

        public ICollection<ExerciseModel>? Exercices { get; set; }

        public Guid VideoTimeCodeId { get; set; }
    }
}
