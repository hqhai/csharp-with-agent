namespace Fsel.Course.Common.Models.Entities
{
    public class TimeCodeExcerciseModel
    {
        public Guid ExcerciseId { get; set; }

        public List<ExerciseModel>? Exercices { get; set; }

        public Guid VideoTimeCodeId { get; set; }
    }
}