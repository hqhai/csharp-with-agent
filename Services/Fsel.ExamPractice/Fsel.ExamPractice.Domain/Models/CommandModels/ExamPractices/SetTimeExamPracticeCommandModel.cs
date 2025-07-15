namespace Fsel.ExamPractice.Domain.Models.CommandModels.ExamPractices
{
    using Fsel.Shared.Enums;

    public class SetTimeExamPracticeCommandModel
    {
        public string? Type { get; set; }
        public Guid ObjectId { get; set; }
        public double AccessTime { get; set; }
        public EnumSubmissionCount? SubmissionCount { get; set; }
    }
}
