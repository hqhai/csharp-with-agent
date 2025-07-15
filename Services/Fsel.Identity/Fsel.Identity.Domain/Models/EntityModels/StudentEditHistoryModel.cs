using Fsel.Core.Base.BaseModels;
using Fsel.Shared.Enums;

namespace Fsel.Identity.Domain.Models.EntityModels
{
    public class StudentEditHistoryModel : BaseModel
    {
        public EnumStudentEditHistoryType Type { get; set; }
        public Guid StudentId { get; set; }
        public StudentEditHistoryDetailModel? EditDetail { get; set; }
        public string? Description { get; set; }
    }

    public class StudentEditHistoryDetailModel
    {
        public DateTime? OldExpiredDate { get; set; }
        public DateTime? NewExpiredDate { get; set; }
    }
}
