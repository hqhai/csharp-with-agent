using System.ComponentModel.DataAnnotations.Schema;
using DataAnnotationsExtensions;
using Fsel.Core.Entities;
using Fsel.Course.Domain.Enums;
using Fsel.Course.Domain.Enums.ErrorCodes;

namespace Fsel.Course.Domain.Entities
{
    public class VideoTimeCode : Entity
    {
        /// <summary>
        /// Thời gian bắt đầu xuất hiện TimeCode
        /// </summary>
        [Min(1, ErrorMessage = nameof(EnumVideoTimeCodeErrorCode.VTC04C))]
        public long DisplayTime { get; set; }

        /// <summary>
        /// Thời gian hiện làm bài
        /// </summary>
        [Min(1, ErrorMessage = nameof(EnumVideoTimeCodeErrorCode.VTC03C))]
        public long ExecutionTime { get; set; }

        /// <summary>
        /// Loại TimeCode
        /// </summary>
        public EnumTimeCodeType TimeCodeType { get; set; }

        public Video? Video { get; set; }
        public Guid VideoId { get; set; }

        [NotMapped]
        public TimeSpan DisplayTimeSpan
        {
            get { return TimeSpan.FromTicks(DisplayTime); }
        }

        [NotMapped]
        public TimeSpan ExecutionTimeSpan
        {
            get { return TimeSpan.FromTicks(ExecutionTime); }
        }

        public ICollection<TimeCodeExcercise> TimeCodeExcercises { get; set; } = new List<TimeCodeExcercise>();
    }
}
