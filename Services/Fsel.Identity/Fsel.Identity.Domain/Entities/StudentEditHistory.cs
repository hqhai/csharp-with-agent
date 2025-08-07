using Fsel.Common.Helpers;
using System.ComponentModel.DataAnnotations.Schema;
using Fsel.Core.Entities;
using Fsel.Shared.Enums;
using System.ComponentModel.DataAnnotations;
using Fsel.Common.Enums.ErrorCodes;

namespace Fsel.Identity.Domain.Entities
{
    public class StudentEditHistory : Entity
    {
        public EnumStudentEditHistoryType Type { get; set; }
        public Guid StudentId { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(1000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? EditDetailStr { get; set; }

        [NotMapped]
        public StudentEditHistoryDetail? EditDetail
        {
            get { return ConvertHelper.Deserialize<StudentEditHistoryDetail>(EditDetailStr); }
            set { EditDetailStr = ConvertHelper.Serialize(value); }
        }

        [MaxLength(500, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Description { get; set; }
    }

    public class StudentEditHistoryDetail
    {
        public DateTime? OldExpiredDate { get; set; }
        public DateTime? NewExpiredDate { get; set; }
    }
}
