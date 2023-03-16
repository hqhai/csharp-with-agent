using System.ComponentModel.DataAnnotations;
using Fsel.Core.Entities;
using Fsel.Course.Domain.Enums.ErrorCodes;

namespace Fsel.Course.Domain.Entities
{
    public class TimeCodeExcercise : Entity
    {
        public VideoTimeCode? VideoTimeCode { get; set; }
        public Excercise? Excercise { get; set; }

        [Required(ErrorMessage = nameof(EnumExcerciseErrorCode.E01V))]
        public Guid ExcerciseId { get; set; }

        [Required(ErrorMessage = nameof(EnumVideoTimeCodeErrorCode.VTC01V))]
        public Guid VideoTimeCodeId { get; set; }
    }
}
