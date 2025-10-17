using Fsel.Common.Enums.ErrorCodes;
using System.ComponentModel.DataAnnotations;
using Fsel.Core.Entities;

namespace Fsel.Course.Domain.Entities
{
    public class VideoSubFilePath : Entity
    {
        public Guid VideoId { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Language { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(1000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? SubFilePath { get; set; }

        public Video? Video { get; set; }
    }
}
