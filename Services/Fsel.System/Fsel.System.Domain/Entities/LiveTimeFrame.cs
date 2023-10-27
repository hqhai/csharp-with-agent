using System.ComponentModel.DataAnnotations;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Core.Entities;

namespace Fsel.System.Domain.Entities
{
    public class LiveTimeFrame : Entity
    {
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public double? StartTime { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public double? EndTime { get; set; }
    }
}
