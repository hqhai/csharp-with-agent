namespace Fsel.Identity.Domain.Models.CommandModels.StudentEditHistory
{
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;

    public class CreateStudentEditHistoryCommandModel
    {
        public EnumStudentEditHistoryType Type { get; set; }
        public Guid StudentId { get; set; }
        public StudentEditHistoryDetailModel? EditDetail { get; set; }
        public string? Description { get; set; }
    }
}
