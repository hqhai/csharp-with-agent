using System.ComponentModel.DataAnnotations;
using Fsel.Common.Enums;
using Fsel.Core.Entities;
using Fsel.Course.Domain.Enums;
using Fsel.Course.Domain.Enums.ErrorCodes;

namespace Fsel.Course.Domain.Entities
{
    public class Video : Entity
    {
        /// <summary>
        /// Tên Video
        /// </summary>
        [Required(ErrorMessage = nameof(EnumLessonErrorCode.LS01C))]
        [MaxLength(250, ErrorMessage = nameof(EnumLessonErrorCode.LS02C))]
        public string? Name { get; set; }

        /// <summary>
        /// Link Video
        /// </summary>
        [MaxLength(1000, ErrorMessage = nameof(EnumLessonErrorCode.LS03C))]
        public string? VideoFilePath { get; set; }

        /// <summary>
        /// Trạng thái kích hoạt
        /// </summary>
        public bool IsActive { get; set; }

        public EnumVideoType Type { get; set; }

        /// <summary>
        /// Giáo Viên ID
        /// </summary>
        public Guid? TeacherId { get; set; }

        /// <summary>
        /// Trình độ khóa
        /// </summary>
        public EnumCourseLevel CourseLevel { get; set; }

        public List<LessonVideo> LessonVideos { get; set; } = new List<LessonVideo>();
        public List<VideoTimeCode> VideoTimeCodes { get; set; } = new List<VideoTimeCode>();
    }
}
