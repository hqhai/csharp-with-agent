namespace Fsel.Course.Domain.Models.EntiyModels
{
    public class TimeCodeExcerciseModel
    {
        public Guid ExcerciseId { get; set; }

        public List<ExerciseModel>? Exercices { get; set; }

        public Guid VideoTimeCodeId { get; set; }
    }
}
