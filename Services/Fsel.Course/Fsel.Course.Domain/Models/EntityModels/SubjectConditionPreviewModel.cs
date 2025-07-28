namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Course.Domain.Enums;

    public class SubjectConditionPreviewModel
    {
        public EnumSubjectConditionValueType Type { get; set; }

        public Guid LevelId { get; set; }

        public string? LevelName { get; set; }
    }
}
