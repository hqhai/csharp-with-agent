using System.ComponentModel.DataAnnotations;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Core.Entities;

namespace Fsel.System.Domain.Entities
{
    public class LiveTimeFrame : Entity
    {
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public DateTime? StartTime { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public DateTime? EndTime { get; set; }
    }
}
